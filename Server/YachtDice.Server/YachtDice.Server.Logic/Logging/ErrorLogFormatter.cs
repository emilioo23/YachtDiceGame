using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace YachtDice.Server.Logic.Logging
{
    /// <summary>
    /// Builds the text of an error for the log. The text has what happened, the type of the exception and of its
    /// original cause, and the first 3 levels of the stack (Stack 3 of the standard) written only as class and
    /// method. It never has the message of the exception, the paths of the computer, the numbers of the lines or
    /// the parameters of the methods, because a person who reads the log could use them to prepare an attack.
    /// </summary>
    public static class ErrorLogFormatter
    {
        private const int MaxStackLevels = 3;
        private const string NullExceptionMessage = "The exception to log cannot be null.";
        private const string ErrorFormat = "{0} Exception: {1}.";
        private const string CauseFormat = " Cause: {0}.";
        private const string StackFormat = " Stack: {0}.";
        private const string LevelFormat = "{0}.{1}";
        private const string LevelSeparator = ", ";

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
                throw new ArgumentNullException(nameof(exception), NullExceptionMessage);
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
