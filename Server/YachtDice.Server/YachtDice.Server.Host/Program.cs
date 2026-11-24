using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using YachtDice.Server.Data.Exceptions;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Host.Composition;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Rooms;
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
        private const string WarmUpEmail = "warm-up@yachtdice.local";
        private const string UnhandledErrorLogMessage = "An error that no service handled is closing the server.";
        private const string WarmUpFailedLogMessage =
            "The database could not be warmed up; the first request will be slower.";

        private static readonly ManualResetEvent _quitEvent = new ManualResetEvent(false);

        /// <summary>
        /// Builds the dependency injection container, opens the service hosts and keeps the application running
        /// until stopped.
        /// </summary>
        private static void Main()
        {
            var logger = new AppLogger(typeof(Program));
            var serviceHosts = new List<ServiceHost>();
            ServiceProvider provider = null;

            Console.CancelKeyPress += HandleCancelKeyPress;
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

            try
            {
                provider = new ServiceCollection().AddServerComponents().BuildServiceProvider();
                WarmUpDatabase(provider, logger);
                GameCoordinator gameCoordinator = OpenServiceHosts(provider, serviceHosts);
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
            catch (Exception ex) when (IsConfigurationFailure(ex))
            {
                ReportFailure(logger, ex, ConfigurationFailedLogMessage);
            }
            finally
            {
                AbortServiceHosts(serviceHosts);
                provider?.Dispose();
            }
        }

        private static bool IsConfigurationFailure(Exception exception)
        {
            return exception is InvalidOperationException
                || exception is ArgumentException
                || exception is IOException
                || exception is XmlException;
        }

        private static void WarmUpDatabase(IServiceProvider provider, AppLogger logger)
        {
            try
            {
                provider.GetRequiredService<IPlayerRepository>().EmailExists(WarmUpEmail);
            }
            catch (DataAccessException ex)
            {
                logger.Error(ex, WarmUpFailedLogMessage);
            }
        }

        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = new AppLogger(typeof(Program));

            logger.Error((Exception)e.ExceptionObject, UnhandledErrorLogMessage);
        }

        private static void HandleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _quitEvent.Set();
        }

        private static GameCoordinator OpenServiceHosts(IServiceProvider provider, List<ServiceHost> serviceHosts)
        {
            var gameCoordinator = provider.GetRequiredService<GameCoordinator>();

            foreach (Type serviceType in ServiceRegistration.HostedServiceTypes)
            {
                serviceHosts.Add(new ServiceHost(provider.GetRequiredService(serviceType)));
            }

            gameCoordinator.RestoreSavedGames(provider.GetRequiredService<IRoomManager>());
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
    }
}
