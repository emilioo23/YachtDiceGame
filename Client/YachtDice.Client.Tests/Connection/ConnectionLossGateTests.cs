using System;
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

            bool[] results = Enumerable.Range(0, Callers)
                .AsParallel()
                .Select(caller => gate.TryEnter())
                .ToArray();
            Func<bool, bool> isTrue = isFirst => isFirst;

            Assert.Equal(1, results.Count(isTrue));
        }
    }
}
