using System;
using System.Runtime.CompilerServices;
using Xunit;
using YachtDice.Server.Logic.Logging;

namespace YachtDice.Server.Tests.Logging
{
    /// <summary>
    /// The text of an error in the log: what happened, the types of the exceptions and the first 3 levels of the
    /// stack as class and method, nothing else.
    /// </summary>
    public class ErrorLogFormatterTests
    {
        private const string Message = "The result of a game could not be saved.";
        private const string SecretPath = "C:\\Projects\\Game\\Server\\Repository.cs";
        private const string ErrorWithoutCauseExpected =
            "The result of a game could not be saved. Exception: InvalidOperationException.";
        private const string ErrorWithCauseExpected =
            "The result of a game could not be saved. Exception: InvalidOperationException. Cause: ArgumentException.";
        private const string ShortStackExpected =
            ErrorWithoutCauseExpected
            + " Stack: ErrorLogFormatterTests.ThrowFailure, ErrorLogFormatterTests.CatchShortFailure.";
        private const string LongStackExpected =
            ErrorWithoutCauseExpected
            + " Stack: ErrorLogFormatterTests.ThrowFailure, ErrorLogFormatterTests.CallFromLevelTwo,"
            + " ErrorLogFormatterTests.CallFromLevelThree.";

        [Fact]
        public void Format_ExceptionWithoutCause_WritesWhatHappenedAndTheTypeOfTheException()
        {
            var exception = new InvalidOperationException(SecretPath);

            string textResult = ErrorLogFormatter.Format(exception, Message);

            Assert.Equal(ErrorWithoutCauseExpected, textResult);
        }

        [Fact]
        public void Format_ExceptionWithCause_WritesTheTypeOfTheOriginalCause()
        {
            var exception = new InvalidOperationException(SecretPath, new ArgumentException(SecretPath));

            string textResult = ErrorLogFormatter.Format(exception, Message);

            Assert.Equal(ErrorWithCauseExpected, textResult);
        }

        [Fact]
        public void Format_ExceptionWithSeveralLevels_WritesOnlyTheTypeOfTheOriginalCause()
        {
            var exception = new InvalidOperationException(
                SecretPath,
                new FormatException(SecretPath, new ArgumentException(SecretPath)));

            string textResult = ErrorLogFormatter.Format(exception, Message);

            Assert.Equal(ErrorWithCauseExpected, textResult);
        }

        [Fact]
        public void Format_ExceptionThrownInTwoLevels_WritesBothLevelsWithoutPathsOrLines()
        {
            string textResult = CatchShortFailure();

            Assert.Equal(ShortStackExpected, textResult);
        }

        [Fact]
        public void Format_ExceptionThrownInFourLevels_WritesOnlyTheFirstThreeLevels()
        {
            string textResult = CatchLongFailure();

            Assert.Equal(LongStackExpected, textResult);
        }

        [Fact]
        public void Format_NullException_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ErrorLogFormatter.Format(null, Message));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowFailure()
        {
            throw new InvalidOperationException(SecretPath);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CallFromLevelTwo()
        {
            ThrowFailure();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CallFromLevelThree()
        {
            CallFromLevelTwo();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string CatchShortFailure()
        {
            string text = string.Empty;

            try
            {
                ThrowFailure();
            }
            catch (InvalidOperationException ex)
            {
                text = ErrorLogFormatter.Format(ex, Message);
            }

            return text;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string CatchLongFailure()
        {
            string text = string.Empty;

            try
            {
                CallFromLevelThree();
            }
            catch (InvalidOperationException ex)
            {
                text = ErrorLogFormatter.Format(ex, Message);
            }

            return text;
        }
    }
}
