using System;
using System.Collections.Generic;
using System.Reflection;
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
    internal static class Program
    {
        private const string StartedLogMessage = "Server started. Press Enter to stop it.";
        private const string StoppedLogMessage = "Server stopped.";
        private const string CommunicationFailedLogMessage = "The services could not be started or stopped.";
        private const string ConfigurationFailedLogMessage = "The service configuration is not valid.";

        private static void Main()
        {
            Console.WriteLine("=== DIAGNÓSTICO DE RECURSOS INCRUSTADOS ===");

            // Usamos PlayerRepository porque sabemos que pertenece al ensamblado YachtDice.Server.Data
            Assembly assembly = typeof(PlayerRepository).Assembly;
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (string name in resourceNames)
            {
                if (name.EndsWith(".csdl") || name.EndsWith(".ssdl") || name.EndsWith(".msl"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(name);
                    Console.ResetColor();
                }
            }
            Console.WriteLine("===========================================\n");

            var logger = new AppLogger(typeof(Program));
            var serviceHosts = new List<ServiceHost>();

            try
            {
                serviceHosts.Add(new ServiceHost(CreateAuthenticationService()));
                serviceHosts.Add(new ServiceHost(CreatePlayerService()));
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

        private static AuthenticationService CreateAuthenticationService()
        {
            var playerRepository = new PlayerRepository();
            var codeService = new TwoFactorCodeService();
            var deliveryService = new TwoFactorDeliveryService(codeService, new EmailService());
            var verificationService = new TwoFactorVerificationService(playerRepository, codeService, deliveryService);

            return new AuthenticationService(
                new LoginService(playerRepository, codeService, deliveryService, verificationService),
                new RegistrationService(playerRepository),
                verificationService);
        }

        private static PlayerService CreatePlayerService()
        {
            var gameHistoryRepository = new GameHistoryRepository();
            var statisticsService = new PlayerStatisticsService(new PlayerRepository(), gameHistoryRepository);

            return new PlayerService(statisticsService, new LeaderboardService(gameHistoryRepository));
        }
    }
}