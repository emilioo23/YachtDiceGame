using System;
using System.Collections.Generic;
using System.ServiceModel;
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
        private const string StartedLogMessage = "Server started. Press Enter to stop it.";
        private const string StoppedLogMessage = "Server stopped.";
        private const string CommunicationFailedLogMessage = "The services could not be started or stopped.";
        private const string ConfigurationFailedLogMessage = "The service configuration is not valid.";

        /// <summary>
        /// Initializes and opens the service hosts, keeping the application running until stopped.
        /// </summary>
        private static void Main()
        {
            var logger = new AppLogger(typeof(Program));
            var serviceHosts = new List<ServiceHost>();

            try
            {
                serviceHosts.Add(new ServiceHost(CreateAuthenticationService()));
                serviceHosts.Add(new ServiceHost(CreatePlayerRegistrationService()));
                serviceHosts.Add(new ServiceHost(CreatePlayerService()));
                serviceHosts.Add(new ServiceHost(new GameService()));
                serviceHosts.ForEach(serviceHost => serviceHost.Open());
                logger.Info(StartedLogMessage);
                Console.ReadLine();
                serviceHosts.ForEach(serviceHost => serviceHost.Close());
                logger.Info(StoppedLogMessage);
            }
            catch (CommunicationException ex)
            {
                logger.Error(ex, CommunicationFailedLogMessage);
                Console.ReadLine();
            }
            catch (InvalidOperationException ex)
            {
                logger.Error(ex, ConfigurationFailedLogMessage);
                Console.ReadLine();
            }
            finally
            {
                serviceHosts.ForEach(serviceHost => serviceHost.Abort());
            }
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
                new LoginService(playerRepository, codeService, deliveryService, verificationService),
                verificationService);
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