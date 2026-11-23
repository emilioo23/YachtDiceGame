using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Avatars;
using YachtDice.Server.Logic.Configuration;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.Friends;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Moderation;
using YachtDice.Server.Logic.Passkeys;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Logic.TwoFactor;
using YachtDice.Server.Services;

namespace YachtDice.Server.Host.Composition
{
    /// <summary>
    /// Registers the components of the server in the dependency injection container. The components that keep
    /// shared state protected by locks are singletons; the ones without state are transient.
    /// </summary>
    internal static class ServiceRegistration
    {
        private const string MaxFriendsKey = "Friends.MaxFriends";
        private const string MaxPendingRequestsKey = "Friends.MaxPendingRequests";
        private const string RoomWaitingMinutesKey = "Rooms.WaitingMinutes";
        private const string InvalidNumberFormat = "The setting {0} is missing or is not a positive whole number.";
        private const int SmallestAllowedNumber = 1;
        private const string OffensiveWordsPath = "Resources\\OffensiveWords.txt";
        private const string GameRulesPath = "Resources\\GameRules.xml";

        /// <summary>
        /// Gets the WCF services that the server publishes, in the order their hosts are opened.
        /// </summary>
        public static IReadOnlyList<Type> HostedServiceTypes { get; } = new[]
        {
            typeof(AuthenticationService),
            typeof(TwoFactorManagementService),
            typeof(PlayerRegistrationService),
            typeof(PlayerService),
            typeof(RoomService),
            typeof(GameService),
            typeof(ChatService),
            typeof(PresenceService),
            typeof(FriendService)
        };

        /// <summary>
        /// Registers every component that the WCF services of the server need.
        /// </summary>
        /// <param name="services">The collection of the container.</param>
        /// <returns>The same collection, with the components registered.</returns>
        public static IServiceCollection AddServerComponents(this IServiceCollection services)
        {
            AddRepositories(services);
            AddSharedComponents(services);
            AddAccountComponents(services);
            AddGameComponents(services);
            AddFriendComponents(services);
            AddHostedServices(services);

            return services;
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddTransient<IPlayerRepository, PlayerRepository>();
            services.AddTransient<IBackupCodeRepository, BackupCodeRepository>();
            services.AddTransient<IPasskeyRepository, PasskeyRepository>();
            services.AddTransient<IPasswordResetRepository, PasswordResetRepository>();
            services.AddTransient<IGameHistoryRepository, GameHistoryRepository>();
            services.AddTransient<IGameCheckpointRepository, GameCheckpointRepository>();
            services.AddTransient<IKickLogRepository, KickLogRepository>();
            services.AddTransient<IFriendRepository, FriendRepository>();
            services.AddTransient<IPlayerReportRepository, PlayerReportRepository>();
            services.AddTransient<IRoomInviteRepository, RoomInviteRepository>();
            services.AddTransient<IEmailService, EmailService>();
        }

        private static void AddSharedComponents(IServiceCollection services)
        {
            GameRules gameRules = LoadGameRules();

            services.AddSingleton<SessionRegistry>();
            services.AddSingleton<TwoFactorCodeService>();
            services.AddSingleton<PasskeyChallengeStore>();
            services.AddSingleton(CreateOffensiveWordFilter());
            services.AddSingleton(gameRules.LevelTable);
            services.AddSingleton(gameRules.TurnDuration);
            services.AddSingleton(gameRules.Achievements);
        }

        private static void AddAccountComponents(IServiceCollection services)
        {
            services.AddTransient<TwoFactorVerificationService>();
            services.AddTransient<IVerificationService, TwoFactorVerificationService>();
            services.AddTransient<IPasswordRecoveryService, PasswordRecoveryService>();
            services.AddTransient<AccountLockNotifier>();
            services.AddTransient(CreateLoginService);
            services.AddTransient<PasswordChangeService>();
            services.AddTransient<IPasskeyService, PasskeyService>();
            services.AddTransient<ITwoFactorSettingsService, TwoFactorSettingsService>();
            services.AddTransient<IRegistrationService, RegistrationService>();
            services.AddTransient<AvatarService>();
            services.AddTransient<IAvatarLookup, AvatarService>();
            services.AddTransient<PlayerStatisticsService>();
            services.AddTransient<PlayerAchievements>();
            services.AddTransient<ProfileService>();
        }

        private static void AddGameComponents(IServiceCollection services)
        {
            services.AddSingleton(CreateRoomManager);
            services.AddTransient<KickHistoryService>();
            services.AddTransient<IDieRoller, RandomDieRoller>();
            services.AddTransient<IGameResultRecorder, GameResultRecorder>();
            services.AddSingleton<IGameManager, GameManager>();
            services.AddSingleton<IGameCheckpointService, GameCheckpointService>();
            services.AddSingleton<GameCoordinator>();
        }

        private static void AddFriendComponents(IServiceCollection services)
        {
            services.AddTransient(CreateFriendshipService);
            services.AddTransient<IPlayerIdFinder>(provider => provider.GetRequiredService<IFriendshipService>());
            services.AddTransient<IRoomInvitationService, RoomInvitationService>();
            services.AddSingleton<PresenceService>();
            services.AddSingleton<IPlayerNotifier>(GetPresenceService);
        }

        private static void AddHostedServices(IServiceCollection services)
        {
            services.AddSingleton(CreateWithSessions<AuthenticationService>);
            services.AddSingleton(CreateWithSessions<TwoFactorManagementService>);
            services.AddSingleton(CreateWithSessions<PlayerRegistrationService>);
            services.AddSingleton(CreateWithSessions<PlayerService>);
            services.AddSingleton(CreateWithSessions<RoomService>);
            services.AddSingleton(CreateWithSessions<GameService>);
            services.AddSingleton(CreateWithSessions<ChatService>);
            services.AddSingleton(CreateWithSessions<FriendService>);
        }

        private static TService CreateWithSessions<TService>(IServiceProvider provider)
            where TService : ServiceBase
        {
            TService service = ActivatorUtilities.CreateInstance<TService>(provider);

            service.Sessions = provider.GetRequiredService<SessionRegistry>();
            service.PlayerIds = provider.GetRequiredService<IFriendshipService>();

            return service;
        }

        private static ILoginService CreateLoginService(IServiceProvider provider)
        {
            LoginService loginService = ActivatorUtilities.CreateInstance<LoginService>(provider);
            var lockNotifier = provider.GetRequiredService<AccountLockNotifier>();

            loginService.AccountLockedForTheDay += (player, culture) => lockNotifier.Send(player, culture);

            return loginService;
        }

        private static IRoomManager CreateRoomManager(IServiceProvider provider)
        {
            RoomManager roomManager = ActivatorUtilities.CreateInstance<RoomManager>(provider);

            roomManager.WaitingMinutes = ReadPositiveNumber(RoomWaitingMinutesKey);

            return roomManager;
        }

        private static IFriendshipService CreateFriendshipService(IServiceProvider provider)
        {
            FriendshipService friendshipService = ActivatorUtilities.CreateInstance<FriendshipService>(provider);

            friendshipService.MaxFriends = ReadPositiveNumber(MaxFriendsKey);
            friendshipService.MaxPendingRequests = ReadPositiveNumber(MaxPendingRequestsKey);

            return friendshipService;
        }

        private static IPlayerNotifier GetPresenceService(IServiceProvider provider)
        {
            return provider.GetRequiredService<PresenceService>();
        }

        /// <summary>
        /// Reads a whole number greater than zero, such as a limit or a time of a rule.
        /// </summary>
        /// <param name="key">Key of the setting.</param>
        /// <returns>The configured number.</returns>
        /// <exception cref="InvalidOperationException">The setting is missing or is not a positive whole
        /// number.</exception>
        private static int ReadPositiveNumber(string key)
        {
            string configuredValue = ConfigurationManager.AppSettings[key];
            bool isNumber = int.TryParse(
                configuredValue,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int value);

            if (!isNumber || value < SmallestAllowedNumber)
            {
                string message = string.Format(CultureInfo.InvariantCulture, InvalidNumberFormat, key);

                throw new InvalidOperationException(message);
            }

            return value;
        }

        /// <summary>
        /// Reads the game modes, the levels and the achievements from the file of the rules of the game.
        /// </summary>
        /// <returns>The rules of the game.</returns>
        /// <exception cref="IOException">The file could not be read.</exception>
        /// <exception cref="XmlException">The file is not a valid XML document.</exception>
        /// <exception cref="InvalidOperationException">A rule of the file is missing or is not valid.</exception>
        private static GameRules LoadGameRules()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, GameRulesPath);

            GameRules gameRules;

            using (XmlReader reader = XmlReader.Create(path))
            {
                XDocument document = XDocument.Load(reader);

                gameRules = GameRules.Read(document);
            }

            return gameRules;
        }

        /// <summary>
        /// Reads the offensive words from the file of the server.
        /// </summary>
        /// <returns>The filter with the words of the file.</returns>
        /// <exception cref="IOException">The file could not be read.</exception>
        private static OffensiveWordFilter CreateOffensiveWordFilter()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OffensiveWordsPath);

            return new OffensiveWordFilter(File.ReadAllLines(path));
        }
    }
}
