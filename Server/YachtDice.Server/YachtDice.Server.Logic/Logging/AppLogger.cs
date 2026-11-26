using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using log4net;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Logic.Logging
{
    /// <summary>
    /// Writes the log of the server and builds the text of the errors.
    /// </summary>
    public class AppLogger
    {
        private const int MaxStackLevels = 3;
        private const string ErrorFormat = "{0} Exception: {1}.";
        private const string CauseFormat = " Cause: {0}.";
        private const string StackFormat = " Stack: {0}.";
        private const string LevelFormat = "{0}.{1}";
        private const string LevelSeparator = ", ";
        private readonly ILog _log;

        /// <summary>
        /// Creates a logger tied to the class that uses it.
        /// </summary>
        /// <param name="ownerType">Type of the class that logs the events.</param>
        public AppLogger(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new ArgumentNullException(nameof(ownerType), LogMessages.NullOwnerTypeMessage);
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
            _log.Warn(AppLogger.Format(exception, message));
        }

        /// <summary>
        /// Logs a failure that prevented an operation from completing. It writes what happened and the type of the
        /// exception and of its original cause, and never the message of the exception, the stack trace or the
        /// paths of the computer (see <see cref="AppLogger"/>).
        /// </summary>
        /// <param name="exception">Caught exception.</param>
        /// <param name="message">Context of the operation that failed.</param>
        public void Error(Exception exception, string message)
        {
            _log.Error(AppLogger.Format(exception, message));
        }

        /// <summary>
        /// Builds the text of an error for the log.
        /// </summary>
        /// <param name="exception">Caught exception.</param>
        /// <param name="message">What was happening when the exception occurred.</param>
        /// <returns>What happened, the types of the exceptions and the first 3 levels of the stack.</returns>
        /// <exception cref="ArgumentNullException">The exception is null.</exception>
        public static string Format(Exception exception, string message)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception), LogMessages.NullExceptionMessage);
            }

            return string.Concat(
                string.Format(CultureInfo.InvariantCulture, ErrorFormat, message, exception.GetType().Name),
                DescribeCause(exception),
                DescribeStack(exception));
        }

        private static string DescribeCause(Exception exception)
        {
            Exception cause = exception.GetBaseException();
            string description = string.Empty;

            if (!ReferenceEquals(cause, exception))
            {
                description = string.Format(CultureInfo.InvariantCulture, CauseFormat, cause.GetType().Name);
            }

            return description;
        }

        private static string DescribeStack(Exception exception)
        {
            StackFrame[] frames = new StackTrace(exception, false).GetFrames() ?? new StackFrame[0];
            Func<MethodBase, bool> hasDeclaringType = method => method != null && method.DeclaringType != null;
            string[] levels = frames
                .Select(frame => frame.GetMethod())
                .Where(hasDeclaringType)
                .Take(MaxStackLevels)
                .Select(method => string.Format(
                    CultureInfo.InvariantCulture,
                    LevelFormat,
                    method.DeclaringType.Name,
                    method.Name))
                .ToArray();
            string description = string.Empty;

            if (levels.Length > 0)
            {
                description = string.Format(
                    CultureInfo.InvariantCulture,
                    StackFormat,
                    string.Join(LevelSeparator, levels));
            }

            return description;
        }
    }
}
