using Xunit;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Tests.Security
{
    /// <summary>
    /// Hash saved in the database instead of the password recovery code.
    /// </summary>
    public class PasswordResetCodeHasherTests
    {
        private const int FirstPlayerId = 1;
        private const int SecondPlayerId = 2;
        private const string Code = "123456";
        private const string OtherCode = "654321";
        private const string ShortCode = "23";
        private const string ShiftedCode = "3";
        private const int TwelfthPlayerId = 12;
        private const int NoPlayerId = 0;

        [Fact]
        public void Compute_SamePlayerAndCode_ReturnsTheSameHash()
        {
            string first = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);
            string second = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);

            Assert.Equal(first, second);
        }

        [Fact]
        public void Compute_SameCodeForTwoPlayers_ReturnsDifferentHashes()
        {
            string first = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);
            string second = PasswordResetCodeHasher.Compute(SecondPlayerId, Code);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void Compute_DifferentCodes_ReturnsDifferentHashes()
        {
            string first = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);
            string second = PasswordResetCodeHasher.Compute(FirstPlayerId, OtherCode);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void Compute_AnyCode_DoesNotReturnThePlainCode()
        {
            string hash = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);

            Assert.DoesNotContain(Code, hash);
        }

        [Fact]
        public void Compute_EmptyCode_ReturnsAHash()
        {
            string hash = PasswordResetCodeHasher.Compute(FirstPlayerId, string.Empty);

            Assert.NotEmpty(hash);
        }

        [Fact]
        public void Compute_NullCode_ReturnsTheSameHashAsAnEmptyCode()
        {
            string expected = PasswordResetCodeHasher.Compute(FirstPlayerId, string.Empty);

            string hash = PasswordResetCodeHasher.Compute(FirstPlayerId, null);

            Assert.Equal(expected, hash);
        }

        [Fact]
        public void Compute_DigitsMovedFromTheCodeToTheIdentifier_ReturnsDifferentHashes()
        {
            string expected = PasswordResetCodeHasher.Compute(FirstPlayerId, ShortCode);

            string hash = PasswordResetCodeHasher.Compute(TwelfthPlayerId, ShiftedCode);

            Assert.NotEqual(expected, hash);
        }

        [Fact]
        public void Compute_SameCodeWithSpacesAround_ReturnsADifferentHash()
        {
            string expected = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);

            string hash = PasswordResetCodeHasher.Compute(FirstPlayerId, " " + Code + " ");

            Assert.NotEqual(expected, hash);
        }

        [Fact]
        public void Compute_ZeroIdentifier_ReturnsADifferentHashThanTheFirstPlayer()
        {
            string expected = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);

            string hash = PasswordResetCodeHasher.Compute(NoPlayerId, Code);

            Assert.NotEqual(expected, hash);
        }
    }
}
