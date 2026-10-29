using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Players;
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
                ReportStatus(logger, StartedLogMessage);
                WaitForExit();
                CloseServiceHosts(serviceHosts);
                ReportStatus(logger, StoppedLogMessage);
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
            serviceHosts.Add(new ServiceHost(CreatePlayerRegistrationService()));
            serviceHosts.Add(new ServiceHost(CreatePlayerService()));
            serviceHosts.Add(new ServiceHost(new RoomService()));
            serviceHosts.Add(new ServiceHost(new GameService()));
            serviceHosts.Add(new ServiceHost(new ChatService()));
            serviceHosts.Add(new ServiceHost(new PresenceService()));

            serviceHosts.ForEach(serviceHost => serviceHost.Open());
        }

        private static void WaitForExit()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Console.ReadLine();
                _quitEvent.Set();
            });

            _quitEvent.WaitOne();
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

        private static void ReportStatus(AppLogger logger, string message)
        {
            logger.Info(message);
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

            return new AuthenticationService(
                new LoginService(playerRepository, verificationService, deliveryService),
                verificationService,
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
        /// Creates and configures the player service with its required dependencies.
        /// </summary>
        /// <returns>A fully configured instance of PlayerService.</returns>
        private static PlayerService CreatePlayerService()
        {
            var gameHistoryRepository = new GameHistoryRepository();
            var statisticsService = new PlayerStatisticsService(new PlayerRepository(), gameHistoryRepository);

            return new PlayerService(statisticsService, new LeaderboardService(gameHistoryRepository));
        }
    }
}
