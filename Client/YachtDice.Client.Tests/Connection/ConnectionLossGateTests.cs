using System.Linq;
using System.Threading.Tasks;
using Xunit;
using YachtDice.Client.Logic.Connection;

namespace YachtDice.Client.Tests.Connection
{
    /// <summary>
    /// Rules that make a lost connection be handled only once.
    /// </summary>
    public class ConnectionLossGateTests
    {
        private const int Callers = 20;
        private const int FirstCaller = 0;
        private const int SingleWinner = 1;

        [Fact]
        public void TryEnter_FirstCall_ReturnsTrue()
        {
            var gate = new ConnectionLossGate();

            bool isFirst = gate.TryEnter();

            Assert.True(isFirst);
        }

        [Fact]
        public void TryEnter_TurnAlreadyTaken_ReturnsFalse()
        {
            var gate = new ConnectionLossGate();
            gate.TryEnter();

            bool isSecond = gate.TryEnter();

            Assert.False(isSecond);
        }

        [Fact]
        public void TryEnter_AfterRelease_ReturnsTrueAgain()
        {
            var gate = new ConnectionLossGate();
            gate.TryEnter();
            gate.Release();

            bool isAgain = gate.TryEnter();

            Assert.True(isAgain);
        }

        [Fact]
        public void TryEnter_ManyCallersAtTheSameTime_LetsOnlyOneIn()
        {
            var gate = new ConnectionLossGate();

            bool[] results = Enumerable.Range(FirstCaller, Callers)
                .AsParallel()
                .Select(caller => gate.TryEnter())
                .ToArray();
            Assert.Equal(SingleWinner, results.Count(isFirst => isFirst));
        }

        [Fact]
        public void TryEnter_ReleaseWithoutHavingEntered_StillLetsOneIn()
        {
            var gate = new ConnectionLossGate();
            gate.Release();

            bool hasEntered = gate.TryEnter();

            Assert.True(hasEntered);
        }

        [Fact]
        public void TryEnter_ReleaseCalledTwice_LetsInOnlyOneAfterwards()
        {
            var gate = new ConnectionLossGate();
            gate.Release();
            gate.Release();
            gate.TryEnter();

            bool hasEntered = gate.TryEnter();

            Assert.False(hasEntered);
        }

        [Fact]
        public void TryEnter_TwoDifferentGates_DoNotAffectEachOther()
        {
            var firstGate = new ConnectionLossGate();
            var secondGate = new ConnectionLossGate();
            firstGate.TryEnter();

            bool hasEntered = secondGate.TryEnter();

            Assert.True(hasEntered);
        }

        [Fact]
        public void TryEnter_ThreeCallsInARow_ReturnsFalseTheLastOne()
        {
            var gate = new ConnectionLossGate();
            gate.TryEnter();
            gate.TryEnter();

            bool hasEntered = gate.TryEnter();

            Assert.False(hasEntered);
        }
    }
}
