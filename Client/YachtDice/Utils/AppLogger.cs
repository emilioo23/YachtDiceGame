using System;
using System.Globalization;
using System.Linq;
using log4net;

namespace YachtDice.Utils
{
    /// <summary>
    /// Game event logger that uses the log levels of the team standard
    /// that the client needs (Warning and Error) on top of log4net.
    /// </summary>
    public class AppLogger
    {
        private const int MaxStackLevels = 3;
        private const string ErrorMessageFormat = "{0} {1}: {2}{3}{4}{5}";
        private const string CauseFormat = " Cause: {0}: {1}";
        private const string NullOwnerTypeMessage = "The type that owns the logger cannot be null.";
        private const string NullExceptionMessage = "The exception to log cannot be null.";

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
        /// Logs an unexpected situation that does not stop the execution.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Warning(string message)
        {
            _log.Warn(message);
        }

        /// <summary>
        /// Logs an unexpected situation that does not stop the execution, with the exception that caused it.
        /// </summary>
        /// <param name="exception">Caught exception.</param>
        /// <param name="message">Context of the situation.</param>
        public void Warning(Exception exception, string message)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception), NullExceptionMessage);
            }

            _log.Warn(BuildErrorMessage(exception, message));
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
                throw new ArgumentNullException(nameof(exception), NullExceptionMessage);
            }

            _log.Error(BuildErrorMessage(exception, message));
        }

        private static string BuildErrorMessage(Exception exception, string message)
        {
            string[] stackLines = (exception.StackTrace ?? string.Empty)
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string limitedStack = string.Join(Environment.NewLine, stackLines.Take(MaxStackLevels));

            return string.Format(
                CultureInfo.InvariantCulture,
                ErrorMessageFormat,
                message,
                exception.GetType().Name,
                exception.Message,
                DescribeRootCause(exception),
                Environment.NewLine,
                limitedStack);
        }

        private static string DescribeRootCause(Exception exception)
        {
            Exception rootCause = exception.GetBaseException();
            string description = string.Empty;

            if (!ReferenceEquals(rootCause, exception))
            {
                description = string.Format(
                    CultureInfo.InvariantCulture,
                    CauseFormat,
                    rootCause.GetType().Name,
                    rootCause.Message);
            }

            return description;
        }
    }
}