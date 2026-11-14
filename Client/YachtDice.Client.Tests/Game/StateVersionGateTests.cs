using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// Rule that discards the states of the game that arrive late.
    /// </summary>
    public class StateVersionGateTests
    {
        private const int NoVersion = 0;
        private const int NegativeVersion = -1;
        private const int FirstVersion = 1;
        private const int FarVersion = 100;
        private const int SecondVersion = 2;
        private const int ThirdVersion = 3;
        private const int FourthVersion = 4;
        private const int FifthVersion = 5;

        [Fact]
        public void TryAccept_FirstState_ReturnsTrue()
        {
            var gate = new StateVersionGate();

            var isAccepted = gate.TryAccept(FirstVersion);

            Assert.True(isAccepted);
        }

        [Fact]
        public void TryAccept_NewerState_ReturnsTrue()
        {
            var gate = new StateVersionGate();
            gate.TryAccept(ThirdVersion);

            var isAccepted = gate.TryAccept(FourthVersion);

            Assert.True(isAccepted);
        }

        [Theory]
        [InlineData(ThirdVersion)]
        [InlineData(SecondVersion)]
        public void TryAccept_RepeatedOrOlderState_ReturnsFalse(int version)
        {
            var gate = new StateVersionGate();
            gate.TryAccept(ThirdVersion);

            var isAccepted = gate.TryAccept(version);

            Assert.False(isAccepted);
        }

        [Fact]
        public void TryAccept_OldStateAfterNewOne_DoesNotLowerTheLastVersion()
        {
            var gate = new StateVersionGate();
            gate.TryAccept(FifthVersion);
            gate.TryAccept(SecondVersion);

            var isAccepted = gate.TryAccept(FourthVersion);

            Assert.False(isAccepted);
        }

        [Fact]
        public void TryAccept_ZeroVersion_ReturnsFalse()
        {
            var gate = new StateVersionGate();

            bool isAccepted = gate.TryAccept(NoVersion);

            Assert.False(isAccepted);
        }

        [Fact]
        public void TryAccept_NegativeVersion_ReturnsFalse()
        {
            var gate = new StateVersionGate();

            bool isAccepted = gate.TryAccept(NegativeVersion);

            Assert.False(isAccepted);
        }

        [Fact]
        public void TryAccept_HighestInteger_ReturnsTrue()
        {
            var gate = new StateVersionGate();

            bool isAccepted = gate.TryAccept(int.MaxValue);

            Assert.True(isAccepted);
        }

        [Fact]
        public void TryAccept_VersionThatSkipsNumbers_ReturnsTrue()
        {
            var gate = new StateVersionGate();
            gate.TryAccept(FirstVersion);

            bool isAccepted = gate.TryAccept(FarVersion);

            Assert.True(isAccepted);
        }

        [Fact]
        public void TryAccept_AnyVersionAfterTheHighestInteger_ReturnsFalse()
        {
            var gate = new StateVersionGate();
            gate.TryAccept(int.MaxValue);

            bool isAccepted = gate.TryAccept(FarVersion);

            Assert.False(isAccepted);
        }
    }
}
