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
        private const int ThreeFailures = 3;
        private const int NoPlayerId = 0;

        private readonly PasswordResetAttemptCounter _counter = new PasswordResetAttemptCounter();

        [Fact]
        public void RegisterFailure_RepeatedFailures_CountsThemInOrder()
        {
            var expectedFailures = new { First = OneFailure, Second = TwoFailures };

            int first = _counter.RegisterFailure(FirstPlayerId);
            int second = _counter.RegisterFailure(FirstPlayerId);

            var actualFailures = new { First = first, Second = second };
            Assert.Equal(expectedFailures, actualFailures);
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

        [Fact]
        public void RegisterFailure_ThreeFailures_ReturnsThree()
        {
            _counter.RegisterFailure(FirstPlayerId);
            _counter.RegisterFailure(FirstPlayerId);

            int failures = _counter.RegisterFailure(FirstPlayerId);

            Assert.Equal(ThreeFailures, failures);
        }

        [Fact]
        public void Reset_PlayerWithoutFailures_CountsTheNextFailureAsTheFirst()
        {
            _counter.Reset(FirstPlayerId);

            int failures = _counter.RegisterFailure(FirstPlayerId);

            Assert.Equal(OneFailure, failures);
        }

        [Fact]
        public void Reset_OnePlayer_KeepsTheFailuresOfTheOtherPlayer()
        {
            _counter.RegisterFailure(FirstPlayerId);
            _counter.RegisterFailure(SecondPlayerId);
            _counter.Reset(FirstPlayerId);

            int failures = _counter.RegisterFailure(SecondPlayerId);

            Assert.Equal(TwoFailures, failures);
        }

        [Fact]
        public void Reset_CalledTwice_CountsTheNextFailureAsTheFirst()
        {
            _counter.RegisterFailure(FirstPlayerId);
            _counter.Reset(FirstPlayerId);
            _counter.Reset(FirstPlayerId);

            int failures = _counter.RegisterFailure(FirstPlayerId);

            Assert.Equal(OneFailure, failures);
        }

        [Fact]
        public void RegisterFailure_PlayerWithZeroIdentifier_CountsTheFailure()
        {
            int failures = _counter.RegisterFailure(NoPlayerId);

            Assert.Equal(OneFailure, failures);
        }
    }
}
