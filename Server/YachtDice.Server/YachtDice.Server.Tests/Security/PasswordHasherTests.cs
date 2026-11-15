using Xunit;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Tests.Security
{
    /// <summary>
    /// Hash saved in the database instead of the password recovery code.
    /// </summary>
    public class PasswordHasherTests
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
            string first = PasswordHasher.ComputeResetCodeHash(FirstPlayerId, Code);
            string second = PasswordHasher.ComputeResetCodeHash(FirstPlayerId, Code);

            Assert.Equal(first, second);
        }

        [Fact]
        public void Compute_SameCodeForTwoPlayers_ReturnsDifferentHashes()
        {
            string first = PasswordHasher.ComputeResetCodeHash(FirstPlayerId, Code);
            string second = PasswordHasher.ComputeResetCodeHash(SecondPlayerId, Code);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void Compute_DifferentCodes_ReturnsDifferentHashes()
        {
            string first = PasswordHasher.ComputeResetCodeHash(FirstPlayerId, Code);
            string second = PasswordHasher.ComputeResetCodeHash(FirstPlayerId, OtherCode);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void Compute_AnyCode_DoesNotReturnThePlainCode()
        {
            string hash = PasswordHasher.ComputeResetCodeHash(FirstPlayerId, Code);

            Assert.DoesNotContain(Code, hash);
        }

        [Fact]
        public void Compute_EmptyCode_ReturnsAHash()
        {
            string hash = PasswordHasher.ComputeResetCodeHash(FirstPlayerId, string.Empty);

            Assert.NotEmpty(hash);
        }

        [Fact]
        public void Compute_NullCode_ReturnsTheSameHashAsAnEmptyCode()
        {
            string emptyCodeHash = PasswordHasher.ComputeResetCodeHash(FirstPlayerId, string.Empty);

            string nullCodeHash = PasswordHasher.ComputeResetCodeHash(FirstPlayerId, null);

            Assert.Equal(emptyCodeHash, nullCodeHash);
        }

        [Fact]
        public void Compute_DigitsMovedFromTheCodeToTheIdentifier_ReturnsDifferentHashes()
        {
            string originalHash = PasswordHasher.ComputeResetCodeHash(FirstPlayerId, ShortCode);

            string shiftedHash = PasswordHasher.ComputeResetCodeHash(TwelfthPlayerId, ShiftedCode);

            Assert.NotEqual(originalHash, shiftedHash);
        }

        [Fact]
        public void Compute_SameCodeWithSpacesAround_ReturnsADifferentHash()
        {
            string trimmedCodeHash = PasswordHasher.ComputeResetCodeHash(FirstPlayerId, Code);

            string spacedCodeHash = PasswordHasher.ComputeResetCodeHash(FirstPlayerId, " " + Code + " ");

            Assert.NotEqual(trimmedCodeHash, spacedCodeHash);
        }

        [Fact]
        public void Compute_ZeroIdentifier_ReturnsADifferentHashThanTheFirstPlayer()
        {
            string firstPlayerHash = PasswordHasher.ComputeResetCodeHash(FirstPlayerId, Code);

            string noPlayerHash = PasswordHasher.ComputeResetCodeHash(NoPlayerId, Code);

            Assert.NotEqual(firstPlayerHash, noPlayerHash);
        }
    }
}
