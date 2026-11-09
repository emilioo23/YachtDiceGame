using Xunit;
using YachtDice.Server.Logic.Authentication;

namespace YachtDice.Server.Tests.Authentication
{
    /// <summary>
    /// Count of the wrong recovery codes of each player.
    /// </summary>
    public class PasswordResetAttemptCounterTests
    {
        private const int FirstPlayerId = 1;
        private const int SecondPlayerId = 2;
        private const int NoFailures = 0;
        private const int OneFailure = 1;
        private const int TwoFailures = 2;

        private readonly PasswordResetAttemptCounter _counter = new PasswordResetAttemptCounter();

        [Fact]
        public void RegisterFailure_RepeatedFailures_CountsThemInOrder()
        {
            int first = _counter.RegisterFailure(FirstPlayerId);
            int second = _counter.RegisterFailure(FirstPlayerId);

            Assert.Equal(OneFailure, first);
            Assert.Equal(TwoFailures, second);
        }

        [Fact]
        public void RegisterFailure_TwoPlayers_CountsEachOneApart()
        {
            _counter.RegisterFailure(FirstPlayerId);

            int secondPlayerFailures = _counter.RegisterFailure(SecondPlayerId);

            Assert.Equal(OneFailure, secondPlayerFailures);
        }

        [Fact]
        public void Reset_PlayerWithFailures_StartsFromZeroAgain()
        {
            _counter.RegisterFailure(FirstPlayerId);

            _counter.Reset(FirstPlayerId);

            Assert.Equal(NoFailures, _counter.GetFailures(FirstPlayerId));
        }
    }
}
