using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.Threading;
using YachtDice.Server.Data.Exceptions;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Avatars;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.Friends;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Moderation;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Logic.TwoFactor;
using YachtDice.Server.Services;

namespace YachtDice.Server.Host
{
    /// <summary>
    /// Represents the main entry point for the WCF service host application.
    /// </summary>
    internal static class Program
    {
        private const string StartedLogMessage = "Server started. Press Enter or Ctrl+C to stop it.";
        private const string StoppedLogMessage = "Server stopped gracefully.";
        private const string CommunicationFailedLogMessage = "The services could not be started or stopped.";
        private const string ConfigurationFailedLogMessage = "The service configuration is not valid.";
        private const string AddressInUseLogMessage = "A TCP port is already in use by another application.";
        private const string TurnSecondsKeyFormat = "TurnSeconds.{0}";
        private const string InvalidNumberFormat = "The setting {0} is missing or is not a positive whole number.";
        private const string MaxFriendsKey = "Friends.MaxFriends";
        private const string MaxPendingRequestsKey = "Friends.MaxPendingRequests";
        private const string RoomWaitingMinutesKey = "Rooms.WaitingMinutes";
        private const int SmallestAllowedNumber = 1;
        private const string LevelExperienceKey = "LevelExperience";
        private const string OffensiveWordsPath = "Resources\\OffensiveWords.txt";
        private const char LevelExperienceSeparator = ',';
        private const string InvalidLevelExperienceMessage =
            "The setting LevelExperience is missing or is not a list of numbers separated by commas.";
        private const string WarmUpEmail = "warm-up@yachtdice.local";
        private const string WarmUpFailedLogMessage =
            "The database could not be warmed up; the first request will be slower.";

        private static readonly ManualResetEvent _quitEvent = new ManualResetEvent(false);

        /// <summary>
        /// Initializes and opens the service hosts, keeping the application running until stopped.
        /// </summary>
        private static void Main()
        {
            var logger = new AppLogger(typeof(Program));
            var serviceHosts = new List<ServiceHost>();

            Console.CancelKeyPress += HandleCancelKeyPress;

            try
            {
                WarmUpDatabase(logger);
                GameCoordinator gameCoordinator = OpenServiceHosts(serviceHosts);
                logger.Info(StartedLogMessage);
                WaitForExit();
                gameCoordinator.KeepGamesForRestart();
                CloseServiceHosts(serviceHosts);
                logger.Info(StoppedLogMessage);
            }
            catch (AddressAlreadyInUseException ex)
            {
                ReportFailure(logger, ex, AddressInUseLogMessage);
            }
            catch (CommunicationException ex)
            {
                ReportFailure(logger, ex, CommunicationFailedLogMessage);
            }
            catch (InvalidOperationException ex)
            {
                ReportFailure(logger, ex, ConfigurationFailedLogMessage);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                ReportFailure(logger, ex, ConfigurationFailedLogMessage);
            }
            catch (ArgumentException ex)
            {
                ReportFailure(logger, ex, ConfigurationFailedLogMessage);
            }
            catch (IOException ex)
            {
                ReportFailure(logger, ex, ConfigurationFailedLogMessage);
            }
            finally
            {
                AbortServiceHosts(serviceHosts);
            }
        }

        private static void WarmUpDatabase(AppLogger logger)
        {
            try
            {
                new PlayerRepository().EmailExists(WarmUpEmail);
            }
            catch (DataAccessException ex)
            {
                logger.Error(ex, WarmUpFailedLogMessage);
            }
        }

        /// <summary>
        /// Reads from the configuration the seconds that a turn lasts for each speed of the game.
        /// </summary>
        /// <returns>The durations of the turns.</returns>
        /// <exception cref="InvalidOperationException">A speed has no valid seconds in the configuration.</exception>
        private static TurnDuration CreateTurnDuration()
        {
            var secondsBySpeed = new Dictionary<GameSpeed, int>();

            foreach (GameSpeed speed in Enum.GetValues(typeof(GameSpeed)))
            {
                string key = string.Format(CultureInfo.InvariantCulture, TurnSecondsKeyFormat, speed);

                secondsBySpeed[speed] = ReadPositiveNumber(key);
            }

            return new TurnDuration(secondsBySpeed);
        }

        /// <summary>
        /// Reads from the configuration a whole number greater than zero, such as a limit or a time of a rule.
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
        /// Reads from the configuration the experience that each level needs, from the first level to the last.
        /// </summary>
        /// <returns>The table of levels.</returns>
        /// <exception cref="InvalidOperationException">The setting is missing or has a value that is not a
        /// number.</exception>
        private static LevelTable CreateLevelTable()
        {
            string configuredExperience = ConfigurationManager.AppSettings[LevelExperienceKey] ?? string.Empty;
            var thresholds = new List<int>();

            foreach (string value in configuredExperience.Split(LevelExperienceSeparator))
            {
                if (!int.TryParse(value.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int experience))
                {
                    throw new InvalidOperationException(InvalidLevelExperienceMessage);
                }

                thresholds.Add(experience);
            }

            return new LevelTable(thresholds);
        }

        /// <summary>
        /// Reads the offensive words from the file of the server, once, when the server starts.
        /// </summary>
        /// <returns>The filter with the words of the file.</returns>
        /// <exception cref="IOException">The file could not be read.</exception>
        private static OffensiveWordFilter CreateOffensiveWordFilter()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OffensiveWordsPath);

            return new OffensiveWordFilter(File.ReadAllLines(path));
        }

        private static void HandleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _quitEvent.Set();
        }

        private static GameCoordinator OpenServiceHosts(List<ServiceHost> serviceHosts)
        {
            var sessions = new SessionRegistry();
            AuthenticationService authenticationService = CreateAuthenticationService();
            TwoFactorManagementService twoFactorService = CreateTwoFactorManagementService();

            authenticationService.Sessions = sessions;
            twoFactorService.Sessions = sessions;
            serviceHosts.Add(new ServiceHost(authenticationService));
            serviceHosts.Add(new ServiceHost(twoFactorService));
            OffensiveWordFilter wordFilter = CreateOffensiveWordFilter();
            serviceHosts.Add(new ServiceHost(CreatePlayerRegistrationService(wordFilter)));

            LevelTable levelTable = CreateLevelTable();
            var avatarService = new AvatarService(new PlayerRepository(), levelTable);
            PlayerService playerService = CreatePlayerService(avatarService, wordFilter, levelTable);

            playerService.Sessions = sessions;
            serviceHosts.Add(new ServiceHost(playerService));

            var roomManager = new RoomManager(avatarService, wordFilter)
            {
                WaitingMinutes = ReadPositiveNumber(RoomWaitingMinutesKey)
            };
            var resultRecorder = new GameResultRecorder(new PlayerRepository(), new GameHistoryRepository());
            var gameManager = new GameManager(new RandomDieRoller(), CreateTurnDuration());
            var checkpointService = new GameCheckpointService(new PlayerRepository(), new GameCheckpointRepository());
            var gameCoordinator = new GameCoordinator(gameManager, resultRecorder, checkpointService);
            var kickHistoryService = new KickHistoryService(new PlayerRepository(), new KickLogRepository());
            var roomService = new RoomService(roomManager, gameCoordinator, kickHistoryService)
            {
                Sessions = sessions
            };
            serviceHosts.Add(new ServiceHost(roomService));
            serviceHosts.Add(new ServiceHost(new GameService(gameCoordinator)));
            serviceHosts.Add(new ServiceHost(new ChatService(roomManager, wordFilter)));

            var presenceService = new PresenceService();
            serviceHosts.Add(new ServiceHost(presenceService));
            FriendService friendService = CreateFriendService(presenceService, roomManager);

            friendService.Sessions = sessions;
            serviceHosts.Add(new ServiceHost(friendService));

            gameCoordinator.RestoreSavedGames(roomManager);
            serviceHosts.ForEach(serviceHost => serviceHost.Open());

            return gameCoordinator;
        }

        private static void WaitForExit()
        {
            ThreadPool.QueueUserWorkItem(WaitForEnterKey);
            _quitEvent.WaitOne();
        }

        private static void WaitForEnterKey(object state)
        {
            Console.ReadLine();
            _quitEvent.Set();
        }

        private static void CloseServiceHosts(List<ServiceHost> serviceHosts)
        {
            foreach (ServiceHost serviceHost in serviceHosts)
            {
                if (serviceHost.State == CommunicationState.Opened)
                {
                    serviceHost.Close();
                }
            }
        }

        private static void AbortServiceHosts(List<ServiceHost> serviceHosts)
        {
            foreach (ServiceHost serviceHost in serviceHosts)
            {
                if (serviceHost.State != CommunicationState.Closed)
                {
                    serviceHost.Abort();
                }
            }
        }

        private static void ReportFailure(AppLogger logger, Exception exception, string message)
        {
            logger.Error(exception, message);
            Console.ReadLine();
        }

        /// <summary>
        /// Creates and configures the authentication service with its required dependencies.
        /// </summary>
        /// <returns>A fully configured instance of AuthenticationService.</returns>
        private static AuthenticationService CreateAuthenticationService()
        {
            var playerRepository = new PlayerRepository();
            var codeService = new TwoFactorCodeService(new BackupCodeRepository());
            var deliveryService = new TwoFactorDeliveryService(codeService, new EmailService());
            var verificationService = new TwoFactorVerificationService(playerRepository, codeService, deliveryService);

            var passwordRecoveryService = new PasswordRecoveryService(
                playerRepository,
                new PasswordResetRepository(),
                new EmailService());

            var loginService = new LoginService(playerRepository, verificationService, deliveryService);
            var lockNotifier = new AccountLockNotifier(new EmailService());

            loginService.AccountLockedForTheDay += (player, culture) => lockNotifier.Send(player, culture);

            return new AuthenticationService(
                loginService,
                verificationService,
                passwordRecoveryService);
        }

        /// <summary>
        /// Creates and configures the two-factor management service with its required dependencies.
        /// </summary>
        /// <returns>A fully configured instance of TwoFactorManagementService.</returns>
        private static TwoFactorManagementService CreateTwoFactorManagementService()
        {
            var playerRepository = new PlayerRepository();
            var codeService = new TwoFactorCodeService(new BackupCodeRepository());
            var deliveryService = new TwoFactorDeliveryService(codeService, new EmailService());
            var verificationService = new TwoFactorVerificationService(playerRepository, codeService, deliveryService);

            var passwordChangeService = new PasswordChangeService(playerRepository, new PasswordResetRepository());

            return new TwoFactorManagementService(
                new TwoFactorSettingsService(playerRepository, verificationService, deliveryService),
                passwordChangeService);
        }

        /// <summary>
        /// Creates and configures the player registration service with its required dependencies.
        /// </summary>
        /// <param name="wordFilter">Filter that rejects the usernames with offensive words.</param>
        /// <returns>A fully configured instance of PlayerRegistrationService.</returns>
        private static PlayerRegistrationService CreatePlayerRegistrationService(OffensiveWordFilter wordFilter)
        {
            var playerRepository = new PlayerRepository();

            var registrationService = new RegistrationService(playerRepository, wordFilter);

            return new PlayerRegistrationService(registrationService);
        }

        /// <summary>
        /// Creates and configures the friend service with its required dependencies.
        /// </summary>
        /// <param name="notifier">Component that warns the players who are online.</param>
        /// <param name="roomManager">The rooms, to know who is inside each one.</param>
        /// <returns>A fully configured instance of FriendService.</returns>
        private static FriendService CreateFriendService(IPlayerNotifier notifier, IRoomManager roomManager)
        {
            var friendshipService = new FriendshipService(
                new PlayerRepository(),
                new FriendRepository(),
                new PlayerReportRepository())
            {
                MaxFriends = ReadPositiveNumber(MaxFriendsKey),
                MaxPendingRequests = ReadPositiveNumber(MaxPendingRequestsKey)
            };
            var invitationService = new RoomInvitationService(
                roomManager,
                friendshipService,
                new RoomInviteRepository());

            return new FriendService(friendshipService, invitationService, notifier);
        }

        /// <summary>
        /// Creates and configures the player service with its required dependencies.
        /// </summary>
        /// <param name="avatarService">Service that handles the avatars of the players.</param>
        /// <param name="wordFilter">Filter that rejects the offensive usernames.</param>
        /// <param name="levelTable">Experience that each level needs.</param>
        /// <returns>A fully configured instance of PlayerService.</returns>
        private static PlayerService CreatePlayerService(
            AvatarService avatarService,
            OffensiveWordFilter wordFilter,
            LevelTable levelTable)
        {
            var gameHistoryRepository = new GameHistoryRepository();
            var statisticsService = new PlayerStatisticsService(
                new PlayerRepository(),
                gameHistoryRepository,
                levelTable);

            var profileService = new ProfileService(new PlayerRepository(), wordFilter, statisticsService);

            return new PlayerService(statisticsService, avatarService, profileService);
        }
    }
}
