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
        private const int OneFailure = 1;
        private const int TwoFailures = 2;

        private readonly PasswordResetAttemptCounter _counter = new PasswordResetAttemptCounter();

        [Fact]
        public void RegisterFailure_RepeatedFailures_CountsThemInOrder()
        {
            var expected = new { First = OneFailure, Second = TwoFailures };

            int first = _counter.RegisterFailure(FirstPlayerId);
            int second = _counter.RegisterFailure(FirstPlayerId);

            var actual = new { First = first, Second = second };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RegisterFailure_TwoPlayers_CountsEachOneApart()
        {
            _counter.RegisterFailure(FirstPlayerId);

            int secondPlayerFailures = _counter.RegisterFailure(SecondPlayerId);

            Assert.Equal(OneFailure, secondPlayerFailures);
        }

        [Fact]
        public void Reset_PlayerWithFailures_CountsTheNextFailureAsTheFirst()
        {
            _counter.RegisterFailure(FirstPlayerId);
            _counter.Reset(FirstPlayerId);

            int failures = _counter.RegisterFailure(FirstPlayerId);

            Assert.Equal(OneFailure, failures);
        }
    }
}
