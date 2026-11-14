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
            string emptyCodeHash = PasswordResetCodeHasher.Compute(FirstPlayerId, string.Empty);

            string nullCodeHash = PasswordResetCodeHasher.Compute(FirstPlayerId, null);

            Assert.Equal(emptyCodeHash, nullCodeHash);
        }

        [Fact]
        public void Compute_DigitsMovedFromTheCodeToTheIdentifier_ReturnsDifferentHashes()
        {
            string originalHash = PasswordResetCodeHasher.Compute(FirstPlayerId, ShortCode);

            string shiftedHash = PasswordResetCodeHasher.Compute(TwelfthPlayerId, ShiftedCode);

            Assert.NotEqual(originalHash, shiftedHash);
        }

        [Fact]
        public void Compute_SameCodeWithSpacesAround_ReturnsADifferentHash()
        {
            string trimmedCodeHash = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);

            string spacedCodeHash = PasswordResetCodeHasher.Compute(FirstPlayerId, " " + Code + " ");

            Assert.NotEqual(trimmedCodeHash, spacedCodeHash);
        }

        [Fact]
        public void Compute_ZeroIdentifier_ReturnsADifferentHashThanTheFirstPlayer()
        {
            string firstPlayerHash = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);

            string noPlayerHash = PasswordResetCodeHasher.Compute(NoPlayerId, Code);

            Assert.NotEqual(firstPlayerHash, noPlayerHash);
        }
    }
}
