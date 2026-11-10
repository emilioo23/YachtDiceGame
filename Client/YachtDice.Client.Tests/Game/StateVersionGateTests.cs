using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// Rule that discards the states of the game that arrive late.
    /// </summary>
    public class StateVersionGateTests
    {
        [Fact]
        public void TryAccept_FirstState_ReturnsTrue()
        {
            var gate = new StateVersionGate();

            var isAccepted = gate.TryAccept(1);

            Assert.True(isAccepted);
        }

        [Fact]
        public void TryAccept_NewerState_ReturnsTrue()
        {
            var gate = new StateVersionGate();
            gate.TryAccept(3);

            var isAccepted = gate.TryAccept(4);

            Assert.True(isAccepted);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(2)]
        public void TryAccept_RepeatedOrOlderState_ReturnsFalse(int version)
        {
            var gate = new StateVersionGate();
            gate.TryAccept(3);

            var isAccepted = gate.TryAccept(version);

            Assert.False(isAccepted);
        }

        [Fact]
        public void TryAccept_OldStateAfterNewOne_DoesNotLowerTheLastVersion()
        {
            var gate = new StateVersionGate();
            gate.TryAccept(5);
            gate.TryAccept(2);

            var isAccepted = gate.TryAccept(4);

            Assert.False(isAccepted);
        }
    }
}
