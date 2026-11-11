using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using log4net;

namespace YachtDice.Server.Logic.Logging
{
    /// <summary>
    /// Server event logger that uses the log levels of the team standard
    /// that the server needs (Info, Warning and Error) on top of log4net.
    /// </summary>
    public class AppLogger
    {
        private const int MaxStackLevels = 3;
        private const string NullOwnerTypeMessage = "The owner type cannot be null when initializing the logger.";
        private const string NullExceptionMessage = "The exception instance cannot be null when logging an error.";
        private const string ErrorMessageFormat = "{0} {1}: {2}{3}{4}{5}";
        private const string CauseFormat = " Cause: {0}: {1}";
        private const string SqlCauseFormat = " Cause: {0} number {1}";

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
        /// Logs a failure that prevented an operation from completing, with the stack trace limited to 3 levels.
        /// It also writes the original cause when the exception wraps another one. For a database error only the
        /// SQL Server error number is written, because its text can include the name of the database login.
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

            if (rootCause is SqlException sqlException)
            {
                description = string.Format(
                    CultureInfo.InvariantCulture,
                    SqlCauseFormat,
                    nameof(SqlException),
                    sqlException.Number);
            }
            else if (!ReferenceEquals(rootCause, exception))
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