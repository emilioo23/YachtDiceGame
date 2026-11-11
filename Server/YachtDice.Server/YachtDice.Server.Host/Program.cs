using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Avatars;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.Friends;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Logic.Rooms;
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
                OpenServiceHosts(serviceHosts);
                logger.Info(StartedLogMessage);
                WaitForExit();
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
            finally
            {
                AbortServiceHosts(serviceHosts);
            }
        }

        private static void HandleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _quitEvent.Set();
        }

        private static void OpenServiceHosts(List<ServiceHost> serviceHosts)
        {
            serviceHosts.Add(new ServiceHost(CreateAuthenticationService()));
            serviceHosts.Add(new ServiceHost(CreateTwoFactorManagementService()));
            serviceHosts.Add(new ServiceHost(CreatePlayerRegistrationService()));

            var avatarService = new AvatarService(new PlayerRepository());
            serviceHosts.Add(new ServiceHost(CreatePlayerService(avatarService)));

            var roomManager = new RoomManager(avatarService);
            var gameCoordinator = new GameCoordinator(new GameManager(new RandomDieSource()));
            serviceHosts.Add(new ServiceHost(new RoomService(roomManager, gameCoordinator)));
            serviceHosts.Add(new ServiceHost(new GameService(gameCoordinator)));
            serviceHosts.Add(new ServiceHost(new ChatService(roomManager)));

            var presenceService = new PresenceService();
            serviceHosts.Add(new ServiceHost(presenceService));
            serviceHosts.Add(new ServiceHost(CreateFriendService(presenceService, roomManager)));

            serviceHosts.ForEach(serviceHost => serviceHost.Open());
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
            var codeService = new TwoFactorCodeService();
            var deliveryService = new TwoFactorDeliveryService(codeService, new EmailService());
            var verificationService = new TwoFactorVerificationService(playerRepository, codeService, deliveryService);

            var passwordRecoveryService = new PasswordRecoveryService(
                playerRepository,
                new PasswordResetRepository(),
                new PasswordResetEmailSender(new EmailService()));

            return new AuthenticationService(
                new LoginService(playerRepository, verificationService, deliveryService),
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
            var codeService = new TwoFactorCodeService();
            var deliveryService = new TwoFactorDeliveryService(codeService, new EmailService());
            var verificationService = new TwoFactorVerificationService(playerRepository, codeService, deliveryService);

            return new TwoFactorManagementService(
                new TwoFactorSettingsService(playerRepository, verificationService, deliveryService));
        }

        /// <summary>
        /// Creates and configures the player registration service with its required dependencies.
        /// </summary>
        /// <returns>A fully configured instance of PlayerRegistrationService.</returns>
        private static PlayerRegistrationService CreatePlayerRegistrationService()
        {
            var playerRepository = new PlayerRepository();

            return new PlayerRegistrationService(new RegistrationService(playerRepository));
        }

        /// <summary>
        /// Creates and configures the friend service with its required dependencies.
        /// </summary>
        /// <param name="notifier">Component that warns the players who are online.</param>
        /// <param name="roomManager">The rooms, to know who is inside each one.</param>
        /// <returns>A fully configured instance of FriendService.</returns>
        private static FriendService CreateFriendService(IPlayerNotifier notifier, IRoomManager roomManager)
        {
            var friendshipService = new FriendshipService(new PlayerRepository(), new FriendRepository());
            var invitationService = new RoomInvitationService(roomManager, friendshipService);

            return new FriendService(friendshipService, invitationService, notifier);
        }

        /// <summary>
        /// Creates and configures the player service with its required dependencies.
        /// </summary>
        /// <param name="avatarService">Service that handles the avatars of the players.</param>
        /// <returns>A fully configured instance of PlayerService.</returns>
        private static PlayerService CreatePlayerService(AvatarService avatarService)
        {
            var gameHistoryRepository = new GameHistoryRepository();
            var statisticsService = new PlayerStatisticsService(new PlayerRepository(), gameHistoryRepository);

            return new PlayerService(statisticsService, new LeaderboardService(gameHistoryRepository), avatarService);
        }
    }
}
