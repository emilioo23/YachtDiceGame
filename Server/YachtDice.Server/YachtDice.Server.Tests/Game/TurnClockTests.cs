using System;
using System.Threading;
using Xunit;
using YachtDice.Server.Services;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// The countdown of a turn: when it ends, and that an error of the expired action does not escape the
    /// timer thread.
    /// </summary>
    public class TurnClockTests
    {
        private const int TurnNumber = 3;
        private const int OneSecond = 1;
        private const int TenSeconds = 10;
        private const int LongWaitMilliseconds = 5000;
        private const int ShortWaitMilliseconds = 2000;
        private const string FailureMessage = "The expired action failed.";

        private readonly ManualResetEventSlim _expired = new ManualResetEventSlim();

        [Fact]
        public void Constructor_TenSeconds_SetsTheDeadlineAfterNow()
        {
            DateTime earliestDeadline = DateTime.UtcNow.AddSeconds(TenSeconds);

            using (var clock = new TurnClock(TurnNumber, TenSeconds, _expired.Set))
            {
                Assert.True(clock.Deadline >= earliestDeadline);
            }
        }

        [Fact]
        public void Constructor_AnyTurn_KeepsTheTurnNumber()
        {
            using (var clock = new TurnClock(TurnNumber, TenSeconds, _expired.Set))
            {
                Assert.Equal(TurnNumber, clock.TurnNumber);
            }
        }

        [Fact]
        public void Expire_ActionThatThrows_KeepsTheServerRunning()
        {
            using (new TurnClock(TurnNumber, OneSecond, FailAfterSignaling))
            {
                bool hasExpired = _expired.Wait(LongWaitMilliseconds);

                Assert.True(hasExpired);
            }
        }

        [Fact]
        public void Dispose_BeforeTheTimeRunsOut_DoesNotRunTheAction()
        {
            var clock = new TurnClock(TurnNumber, OneSecond, _expired.Set);

            clock.Dispose();

            bool hasExpired = _expired.Wait(ShortWaitMilliseconds);

            Assert.False(hasExpired);
        }

        private void FailAfterSignaling()
        {
            _expired.Set();

            throw new InvalidOperationException(FailureMessage);
        }
    }
}
