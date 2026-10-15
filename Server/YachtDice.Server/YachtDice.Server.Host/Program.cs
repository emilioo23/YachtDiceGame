using System;
using System.ServiceModel;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.TwoFactor;
using YachtDice.Server.Services;

namespace YachtDice.Server.Host
{
    internal static class Program
    {
        private const string StartedLogMessage = "Server started. Press Enter to stop it.";
        private const string StoppedLogMessage = "Server stopped.";
        private const string CommunicationFailedLogMessage = "The service could not be started or stopped.";
        private const string ConfigurationFailedLogMessage = "The service configuration is not valid.";

        private static void Main()
        {
            var logger = new AppLogger(typeof(Program));

            try
            {
                using (var serviceHost = new ServiceHost(CreateAuthenticationService()))
                {
                    serviceHost.Open();
                    logger.Info(StartedLogMessage);
                    Console.ReadLine();
                    serviceHost.Close();
                    logger.Info(StoppedLogMessage);
                }
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
        }

        private static AuthenticationService CreateAuthenticationService()
        {
            var playerRepository = new PlayerRepository();
            var codeService = new TwoFactorCodeService();
            var deliveryService = new TwoFactorDeliveryService(codeService, new EmailService());

            return new AuthenticationService(
                new LoginService(playerRepository, codeService, deliveryService),
                new RegistrationService(playerRepository),
                new TwoFactorVerificationService(playerRepository, codeService, deliveryService));
        }
    }
}