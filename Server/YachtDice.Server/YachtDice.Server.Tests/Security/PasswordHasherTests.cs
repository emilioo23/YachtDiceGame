using System;
using Xunit;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Tests.Security
{
    /// <summary>
    /// Hashes saved in the database instead of the passwords and of the password recovery codes.
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
        private const string Password = "Secret123!";
        private const string OtherPassword = "Other456?";
        private const string LegacyHashOfPassword = "94e0f9bc7f5a5225bd141bad5adf9befcc112aef09b88f47a14e20b75a7bbec2";
        private const string MalformedHash = "pbkdf2-sha256$abc";
        private const string UnknownAlgorithmHash = "md5$100000$c2FsdA==$a2V5";
        private const string BrokenBase64Hash = "pbkdf2-sha256$100000$***$***";
        private const string EmptyText = "";

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

        [Fact]
        public void ComputeHash_SamePasswordTwice_ReturnsDifferentHashes()
        {
            string first = PasswordHasher.ComputeHash(Password);
            string second = PasswordHasher.ComputeHash(Password);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void ComputeHash_AnyPassword_DoesNotContainThePlainPassword()
        {
            string hash = PasswordHasher.ComputeHash(Password);

            Assert.DoesNotContain(Password, hash);
        }

        [Fact]
        public void ComputeHash_EmptyPassword_ThrowsArgumentException()
        {
            Action compute = () => PasswordHasher.ComputeHash(EmptyText);

            Assert.Throws<ArgumentException>(compute);
        }

        [Fact]
        public void Verify_RightPassword_ReturnsTrue()
        {
            string hash = PasswordHasher.ComputeHash(Password);

            bool isValid = PasswordHasher.Verify(Password, hash);

            Assert.True(isValid);
        }

        [Fact]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            string hash = PasswordHasher.ComputeHash(Password);

            bool isValid = PasswordHasher.Verify(OtherPassword, hash);

            Assert.False(isValid);
        }

        [Fact]
        public void Verify_RightPasswordAgainstTheOldUnsaltedHash_ReturnsTrue()
        {
            bool isValid = PasswordHasher.Verify(Password, LegacyHashOfPassword);

            Assert.True(isValid);
        }

        [Fact]
        public void Verify_WrongPasswordAgainstTheOldUnsaltedHash_ReturnsFalse()
        {
            bool isValid = PasswordHasher.Verify(OtherPassword, LegacyHashOfPassword);

            Assert.False(isValid);
        }

        [Theory]
        [InlineData(MalformedHash)]
        [InlineData(UnknownAlgorithmHash)]
        [InlineData(BrokenBase64Hash)]
        public void Verify_DamagedSaltedHash_ReturnsFalse(string storedHash)
        {
            bool isValid = PasswordHasher.Verify(Password, storedHash);

            Assert.False(isValid);
        }

        [Fact]
        public void Verify_EmptyPassword_ThrowsArgumentException()
        {
            Action verify = () => PasswordHasher.Verify(EmptyText, LegacyHashOfPassword);

            Assert.Throws<ArgumentException>(verify);
        }

        [Fact]
        public void Verify_EmptyStoredHash_ThrowsArgumentException()
        {
            Action verify = () => PasswordHasher.Verify(Password, EmptyText);

            Assert.Throws<ArgumentException>(verify);
        }
    }
}
