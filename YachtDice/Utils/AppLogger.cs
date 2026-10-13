using System;
using System.Linq;
using log4net;
using log4net.Core;

namespace YachtDice.Utils
{
    /// <summary>
    /// Game event logger that uses the log levels of the team standard
    /// (Trace, Debug, Info, Warning, Error) on top of log4net.
    /// </summary>
    public class AppLogger
    {
        private const int MaxStackLevels = 3;

        private readonly ILog _log;

        /// <summary>
        /// Creates a logger tied to the class that uses it.
        /// </summary>
        /// <param name="ownerType">Type of the class that logs the events.</param>
        public AppLogger(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new ArgumentNullException(nameof(ownerType));
            }

            _log = LogManager.GetLogger(ownerType);
        }

        /// <summary>
        /// Logs the finest detail of the internal flow; for development only.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Trace(string message)
        {
            _log.Logger.Log(typeof(AppLogger), Level.Trace, message, null);
        }

        /// <summary>
        /// Logs information that is useful for debugging during development and testing.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Debug(string message)
        {
            _log.Debug(message);
        }

        /// <summary>
        /// Logs a relevant event of the normal flow.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Info(string message)
        {
            _log.Info(message);
        }

        /// <summary>
        /// Logs an unexpected situation that does not stop the execution.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Warning(string message)
        {
            _log.Warn(message);
        }

        /// <summary>
        /// Logs a failure that prevented an operation from completing, with the stack trace limited to 3 levels.
        /// </summary>
        /// <param name="exception">Caught exception.</param>
        /// <param name="message">Context of the operation that failed.</param>
        public void Error(Exception exception, string message)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            _log.Error(BuildErrorMessage(exception, message));
        }

        private static string BuildErrorMessage(Exception exception, string message)
        {
            string[] stackLines = (exception.StackTrace ?? string.Empty)
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string limitedStack = string.Join(Environment.NewLine, stackLines.Take(MaxStackLevels));

            return $"{message} {exception.GetType().Name}: {exception.Message}{Environment.NewLine}{limitedStack}";
        }
    }
}