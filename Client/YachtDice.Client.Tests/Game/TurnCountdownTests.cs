using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// Countdown that the players see between the states sent by the server.
    /// </summary>
    public class TurnCountdownTests
    {
        private const int TurnSeconds = 45;
        private const int NegativeSeconds = -5;
        private const int TenSeconds = 10;
        private const int NineSeconds = 9;
        private const int LastSecond = 1;
        private const int NoSeconds = 0;

        [Fact]
        public void Reset_Seconds_SetsTheSecondsLeft()
        {
            var countdown = new TurnCountdown();

            countdown.Reset(TurnSeconds);

            Assert.Equal(TurnSeconds, countdown.SecondsLeft);
        }

        [Fact]
        public void Reset_NegativeSeconds_LeavesZero()
        {
            var countdown = new TurnCountdown();

            countdown.Reset(NegativeSeconds);

            Assert.Equal(NoSeconds, countdown.SecondsLeft);
        }

        [Fact]
        public void Tick_SecondsLeft_SubtractsOneSecond()
        {
            var countdown = new TurnCountdown();
            countdown.Reset(TenSeconds);

            countdown.Tick();

            Assert.Equal(NineSeconds, countdown.SecondsLeft);
        }

        [Fact]
        public void Tick_NoSecondsLeft_StaysAtZero()
        {
            var countdown = new TurnCountdown();
            countdown.Reset(LastSecond);
            countdown.Tick();

            countdown.Tick();

            Assert.Equal(NoSeconds, countdown.SecondsLeft);
        }

        [Fact]
        public void Reset_Zero_LeavesZero()
        {
            var countdown = new TurnCountdown();

            countdown.Reset(NoSeconds);

            Assert.Equal(NoSeconds, countdown.SecondsLeft);
        }

        [Fact]
        public void Reset_MinimumInteger_LeavesZero()
        {
            var countdown = new TurnCountdown();

            countdown.Reset(int.MinValue);

            Assert.Equal(NoSeconds, countdown.SecondsLeft);
        }

        [Fact]
        public void Tick_LastSecond_LeavesZero()
        {
            var countdown = new TurnCountdown();
            countdown.Reset(LastSecond);

            countdown.Tick();

            Assert.Equal(NoSeconds, countdown.SecondsLeft);
        }

        [Fact]
        public void Tick_TwiceAfterTheLastSecond_StaysAtZero()
        {
            var countdown = new TurnCountdown();
            countdown.Reset(LastSecond);
            countdown.Tick();

            countdown.Tick();

            Assert.Equal(NoSeconds, countdown.SecondsLeft);
        }

        [Fact]
        public void Reset_AfterSomeTicks_StartsAgainFromTheNewSeconds()
        {
            var countdown = new TurnCountdown();
            countdown.Reset(TenSeconds);
            countdown.Tick();

            countdown.Reset(TurnSeconds);

            Assert.Equal(TurnSeconds, countdown.SecondsLeft);
        }
    }
}
