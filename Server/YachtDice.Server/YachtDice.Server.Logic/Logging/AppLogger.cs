using System;
using log4net;

namespace YachtDice.Server.Logic.Logging
{
    /// <summary>
    /// Server event logger that uses the log levels of the team standard
    /// that the server needs (Info, Warning and Error) on top of log4net.
    /// </summary>
    public class AppLogger
    {
        private const string NullOwnerTypeMessage = "The owner type cannot be null when initializing the logger.";

        private readonly ILog _log;

        /// <summary>
        /// Creates a logger tied to the class that uses it.
        /// </summary>
        /// <param name="ownerType">Type of the class that logs the events.</param>
        public AppLogger(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new ArgumentNullException(nameof(ownerType), NullOwnerTypeMessage);
            }

            _log = LogManager.GetLogger(ownerType);
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
        /// Logs an unexpected situation that does not stop the execution, with the type of the exception that
        /// caused it.
        /// </summary>
        /// <param name="exception">Caught exception.</param>
        /// <param name="message">Context of the situation.</param>
        public void Warning(Exception exception, string message)
        {
            _log.Warn(ErrorLogFormatter.Format(exception, message));
        }

        /// <summary>
        /// Logs a failure that prevented an operation from completing. It writes what happened and the type of the
        /// exception and of its original cause, and never the message of the exception, the stack trace or the
        /// paths of the computer (see <see cref="ErrorLogFormatter"/>).
        /// </summary>
        /// <param name="exception">Caught exception.</param>
        /// <param name="message">Context of the operation that failed.</param>
        public void Error(Exception exception, string message)
        {
            _log.Error(ErrorLogFormatter.Format(exception, message));
        }
    }
}