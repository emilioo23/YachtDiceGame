using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// Countdown that the players see between the states sent by the server.
    /// </summary>
    public class TurnCountdownTests
    {
        [Fact]
        public void Reset_Seconds_SetsTheSecondsLeft()
        {
            var countdown = new TurnCountdown();

            countdown.Reset(45);

            Assert.Equal(45, countdown.SecondsLeft);
            Assert.False(countdown.IsOver);
        }

        [Fact]
        public void Reset_NegativeSeconds_LeavesZero()
        {
            var countdown = new TurnCountdown();

            countdown.Reset(-5);

            Assert.Equal(0, countdown.SecondsLeft);
            Assert.True(countdown.IsOver);
        }

        [Fact]
        public void Tick_SecondsLeft_SubtractsOneSecond()
        {
            var countdown = new TurnCountdown();
            countdown.Reset(10);

            countdown.Tick();

            Assert.Equal(9, countdown.SecondsLeft);
        }

        [Fact]
        public void Tick_NoSecondsLeft_StaysAtZero()
        {
            var countdown = new TurnCountdown();
            countdown.Reset(1);
            countdown.Tick();

            countdown.Tick();

            Assert.Equal(0, countdown.SecondsLeft);
            Assert.True(countdown.IsOver);
        }
    }
}
