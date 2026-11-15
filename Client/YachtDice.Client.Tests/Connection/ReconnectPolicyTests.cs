using Xunit;
using YachtDice.Client.Logic.Connection;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Connection
{
    /// <summary>
    /// The time rules to get back a connection that was lost during a match.
    /// </summary>
    public class ReconnectPolicyTests
    {
        private const int ExpectedRetrySeconds = 5;
        private const int NoSeconds = 0;
        private const int OneSecond = 1;

        [Fact]
        public void CanAttempt_NoTimeElapsed_ReturnsTrue()
        {
            bool canAttempt = ReconnectPolicy.CanAttempt(NoSeconds);

            Assert.True(canAttempt);
        }

        [Fact]
        public void CanAttempt_OneSecondBeforeTheDeadline_ReturnsTrue()
        {
            bool canAttempt = ReconnectPolicy.CanAttempt(ReconnectWaitTracker.ReconnectSeconds - OneSecond);

            Assert.True(canAttempt);
        }

        [Fact]
        public void CanAttempt_ExactlyAtTheDeadline_ReturnsFalse()
        {
            bool canAttempt = ReconnectPolicy.CanAttempt(ReconnectWaitTracker.ReconnectSeconds);

            Assert.False(canAttempt);
        }

        [Fact]
        public void CanAttempt_AfterTheDeadline_ReturnsFalse()
        {
            bool canAttempt = ReconnectPolicy.CanAttempt(ReconnectWaitTracker.ReconnectSeconds + OneSecond);

            Assert.False(canAttempt);
        }

        [Fact]
        public void RetryIntervalSeconds_Constant_IsFiveSeconds()
        {
            Assert.Equal(ExpectedRetrySeconds, ReconnectPolicy.RetryIntervalSeconds);
        }
    }
}
