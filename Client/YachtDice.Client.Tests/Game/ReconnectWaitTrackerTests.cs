using System.Collections.Generic;
using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// The time that the match shows while it waits for an absent player to come back.
    /// </summary>
    public class ReconnectWaitTrackerTests
    {
        private const string FirstId = "player-a";
        private const string SecondId = "player-b";
        private const int SecondsOfWait = 10;
        private const int MoreSecondsThanTheWait = 100;
        private const int NoSeconds = 0;
        private const int OneSecond = 1;

        [Fact]
        public void Update_PlayerWhoJustLeft_StartsWithTheFullTime()
        {
            var tracker = new ReconnectWaitTracker();

            tracker.Update(new[] { FirstId });

            Assert.Equal(ReconnectWaitTracker.ReconnectSeconds, tracker.GetSecondsLeft(FirstId));
        }

        [Fact]
        public void Update_PlayerWhoWasAlreadyAbsent_KeepsTheTimeThatWasLeft()
        {
            var tracker = new ReconnectWaitTracker();
            tracker.Update(new[] { FirstId });
            CountSeconds(tracker, SecondsOfWait);

            tracker.Update(new[] { FirstId });

            Assert.Equal(ReconnectWaitTracker.ReconnectSeconds - SecondsOfWait, tracker.GetSecondsLeft(FirstId));
        }

        [Fact]
        public void Update_PlayerWhoCameBack_IsNotWaitedForAnymore()
        {
            var tracker = new ReconnectWaitTracker();
            tracker.Update(new[] { FirstId, SecondId });

            tracker.Update(new[] { SecondId });

            Assert.Equal(new[] { SecondId }, tracker.GetWaitingPlayerIds());
        }

        [Fact]
        public void Tick_AbsentPlayer_TakesOneSecond()
        {
            var tracker = new ReconnectWaitTracker();
            tracker.Update(new[] { FirstId });

            tracker.Tick();

            Assert.Equal(ReconnectWaitTracker.ReconnectSeconds - OneSecond, tracker.GetSecondsLeft(FirstId));
        }

        [Fact]
        public void Tick_MoreSecondsThanTheWait_NeverGoesBelowZero()
        {
            var tracker = new ReconnectWaitTracker();
            tracker.Update(new[] { FirstId });

            CountSeconds(tracker, MoreSecondsThanTheWait);

            Assert.Equal(NoSeconds, tracker.GetSecondsLeft(FirstId));
        }

        [Fact]
        public void GetWaitingPlayerIds_TimeIsOver_DoesNotListThePlayer()
        {
            var tracker = new ReconnectWaitTracker();
            tracker.Update(new[] { FirstId });
            CountSeconds(tracker, MoreSecondsThanTheWait);

            IReadOnlyList<string> waiting = tracker.GetWaitingPlayerIds();

            Assert.Empty(waiting);
        }

        [Fact]
        public void GetSecondsLeft_PlayerWhoIsNotAbsent_ReturnsZero()
        {
            var tracker = new ReconnectWaitTracker();

            int seconds = tracker.GetSecondsLeft(FirstId);

            Assert.Equal(NoSeconds, seconds);
        }

        [Fact]
        public void TakeExpiredPlayerIds_TimeIsOver_ReturnsThePlayer()
        {
            var tracker = new ReconnectWaitTracker();
            tracker.Update(new[] { FirstId });
            CountSeconds(tracker, MoreSecondsThanTheWait);

            IReadOnlyList<string> expired = tracker.TakeExpiredPlayerIds();

            Assert.Equal(new[] { FirstId }, expired);
        }

        [Fact]
        public void TakeExpiredPlayerIds_PlayerStillHasTime_ReturnsNobody()
        {
            var tracker = new ReconnectWaitTracker();
            tracker.Update(new[] { FirstId });

            IReadOnlyList<string> expired = tracker.TakeExpiredPlayerIds();

            Assert.Empty(expired);
        }

        [Fact]
        public void TakeExpiredPlayerIds_CalledTwice_ReportsThePlayerOnlyOnce()
        {
            var tracker = new ReconnectWaitTracker();
            tracker.Update(new[] { FirstId });
            CountSeconds(tracker, MoreSecondsThanTheWait);
            tracker.TakeExpiredPlayerIds();

            IReadOnlyList<string> expired = tracker.TakeExpiredPlayerIds();

            Assert.Empty(expired);
        }

        private static void CountSeconds(ReconnectWaitTracker tracker, int seconds)
        {
            for (int second = 0; second < seconds; second++)
            {
                tracker.Tick();
            }
        }
    }
}
