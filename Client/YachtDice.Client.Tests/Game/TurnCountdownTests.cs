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
            var expected = new { SecondsLeft = TurnSeconds, IsOver = false };

            countdown.Reset(TurnSeconds);

            var actual = new { countdown.SecondsLeft, countdown.IsOver };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Reset_NegativeSeconds_LeavesZero()
        {
            var countdown = new TurnCountdown();
            var expected = new { SecondsLeft = NoSeconds, IsOver = true };

            countdown.Reset(NegativeSeconds);

            var actual = new { countdown.SecondsLeft, countdown.IsOver };
            Assert.Equal(expected, actual);
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
            var expected = new { SecondsLeft = NoSeconds, IsOver = true };

            countdown.Tick();

            var actual = new { countdown.SecondsLeft, countdown.IsOver };
            Assert.Equal(expected, actual);
        }
    }
}
