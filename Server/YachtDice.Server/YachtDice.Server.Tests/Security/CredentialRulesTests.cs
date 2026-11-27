using System;
using Xunit;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Tests.Security
{
    /// <summary>
    /// Tests of the rules of the credentials and the hash of the passwords.
    /// </summary>
    public class CredentialRulesTests
    {
        private const char FillLetter = 'a';
        private const string Digit = "1";
        private const string UniversityDomain = "@uv.mx";
        private const string EmailWithSurroundingSpaces = " gabriel@gmail.com ";
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
        private const string BlankText = " ";

        [Theory]
        [InlineData("abc")]
        [InlineData("gabriel_hm")]
        [InlineData("emilio.23")]
        [InlineData("DiceKing99")]
        public void IsValidUsername_LettersDigitsDotsAndUnderscores_ReturnsTrue(string username)
        {
            bool isValid = CredentialRules.IsValidUsername(username);

            Assert.True(isValid);
        }

        [Theory]
        [InlineData("Gabriel")]
        [InlineData("Mar\u00EDa Jos\u00E9")]
        [InlineData("Anne-Marie")]
        [InlineData("O'Brien")]
        [InlineData("Hern\u00E1ndez")]
        [InlineData("\u674E\u660E")]
        public void IsValidPersonName_LettersWithSpacesHyphensAndApostrophes_ReturnsTrue(string name)
        {
            bool isValid = CredentialRules.IsValidPersonName(name);

            Assert.True(isValid);
        }

        [Theory]
        [InlineData("Gabriel2")]
        [InlineData("1234")]
        [InlineData("Gab!el")]
        [InlineData("<script>")]
        [InlineData("'; DROP TABLE Player;--")]
        [InlineData("Ana  Lopez")]
        [InlineData("-Ana")]
        [InlineData("Ana-")]
        [InlineData(" Ana")]
        [InlineData("A")]
        [InlineData("")]
        [InlineData(null)]
        public void IsValidPersonName_DigitsSymbolsBadSeparatorsOrTooShort_ReturnsFalse(string name)
        {
            bool isValid = CredentialRules.IsValidPersonName(name);

            Assert.False(isValid);
        }

        [Fact]
        public void IsValidPersonName_NameOfExactlyTheMaximum_ReturnsTrue()
        {
            string name = new string(FillLetter, CredentialRules.NameMaxLength);

            bool isValid = CredentialRules.IsValidPersonName(name);

            Assert.True(isValid);
        }

        [Fact]
        public void IsValidPersonName_NameLongerThanTheMaximum_ReturnsFalse()
        {
            string name = new string(FillLetter, CredentialRules.NameMaxLength + 1);

            bool isValid = CredentialRules.IsValidPersonName(name);

            Assert.False(isValid);
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("gabriel hm")]
        [InlineData("jos\u00E9")]
        [InlineData("dice-king")]
        [InlineData("'; DROP TABLE Player;--")]
        [InlineData("<script>")]
        [InlineData(null)]
        public void IsValidUsername_TooShortOrWithOtherCharacters_ReturnsFalse(string username)
        {
            bool isValid = CredentialRules.IsValidUsername(username);

            Assert.False(isValid);
        }

        [Fact]
        public void IsValidUsername_LongerThanTheLimit_ReturnsFalse()
        {
            string username = new string(FillLetter, CredentialRules.UsernameMaxLength + 1);

            bool isValid = CredentialRules.IsValidUsername(username);

            Assert.False(isValid);
        }

        [Theory]
        [InlineData("Secret123")]
        [InlineData("abcdefg1")]
        [InlineData("contrase\u00F1a 2026")]
        public void IsStrongPassword_LetterDigitAndValidLength_ReturnsTrue(string password)
        {
            bool isStrong = CredentialRules.IsStrongPassword(password);

            Assert.True(isStrong);
        }

        [Theory]
        [InlineData("abc123")]
        [InlineData("onlyletters")]
        [InlineData("12345678")]
        [InlineData("        ")]
        [InlineData(null)]
        public void IsStrongPassword_TooShortOrWithoutLetterAndDigit_ReturnsFalse(string password)
        {
            bool isStrong = CredentialRules.IsStrongPassword(password);

            Assert.False(isStrong);
        }

        [Fact]
        public void IsStrongPassword_LongerThanTheLimit_ReturnsFalse()
        {
            string password = new string(FillLetter, CredentialRules.PasswordMaxLength) + Digit;

            bool isStrong = CredentialRules.IsStrongPassword(password);

            Assert.False(isStrong);
        }

        [Theory]
        [InlineData("gabriel@gmail.com")]
        [InlineData("emilio.alvarez@uv.mx")]
        public void IsValid_WellWrittenEmail_ReturnsTrue(string email)
        {
            Assert.True(CredentialRules.IsValid(email));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        [InlineData("gabriel")]
        [InlineData("gabriel@")]
        [InlineData("@gmail.com")]
        [InlineData("gabriel gmail.com")]
        public void IsValid_EmptyOrMalformedText_ReturnsFalse(string email)
        {
            Assert.False(CredentialRules.IsValid(email));
        }

        [Fact]
        public void IsValid_EmailLongerThanTheMaximum_ReturnsFalse()
        {
            string longEmail = new string('a', CredentialRules.EmailMaxLength) + UniversityDomain;

            Assert.False(CredentialRules.IsValid(longEmail));
        }

        [Fact]
        public void IsValid_EmailWithSpacesAround_ReturnsFalse()
        {
            Assert.False(CredentialRules.IsValid(EmailWithSurroundingSpaces));
        }

        [Fact]
        public void Compute_SamePlayerAndCode_ReturnsTheSameHash()
        {
            string hashExpected = CredentialRules.ComputeResetCodeHash(FirstPlayerId, Code);
            string hashResult = CredentialRules.ComputeResetCodeHash(FirstPlayerId, Code);

            Assert.Equal(hashExpected, hashResult);
        }

        [Fact]
        public void Compute_SameCodeForTwoPlayers_ReturnsDifferentHashes()
        {
            string first = CredentialRules.ComputeResetCodeHash(FirstPlayerId, Code);
            string second = CredentialRules.ComputeResetCodeHash(SecondPlayerId, Code);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void Compute_DifferentCodes_ReturnsDifferentHashes()
        {
            string first = CredentialRules.ComputeResetCodeHash(FirstPlayerId, Code);
            string second = CredentialRules.ComputeResetCodeHash(FirstPlayerId, OtherCode);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void Compute_AnyCode_DoesNotReturnThePlainCode()
        {
            string hash = CredentialRules.ComputeResetCodeHash(FirstPlayerId, Code);

            Assert.DoesNotContain(Code, hash);
        }

        [Fact]
        public void Compute_EmptyCode_ReturnsAHash()
        {
            string hash = CredentialRules.ComputeResetCodeHash(FirstPlayerId, string.Empty);

            Assert.NotEmpty(hash);
        }

        [Fact]
        public void Compute_NullCode_ReturnsTheSameHashAsAnEmptyCode()
        {
            string hashExpected = CredentialRules.ComputeResetCodeHash(FirstPlayerId, string.Empty);

            string hashResult = CredentialRules.ComputeResetCodeHash(FirstPlayerId, null);

            Assert.Equal(hashExpected, hashResult);
        }

        [Fact]
        public void Compute_DigitsMovedFromTheCodeToTheIdentifier_ReturnsDifferentHashes()
        {
            string originalHash = CredentialRules.ComputeResetCodeHash(FirstPlayerId, ShortCode);

            string shiftedHash = CredentialRules.ComputeResetCodeHash(TwelfthPlayerId, ShiftedCode);

            Assert.NotEqual(originalHash, shiftedHash);
        }

        [Fact]
        public void Compute_SameCodeWithSpacesAround_ReturnsADifferentHash()
        {
            string trimmedCodeHash = CredentialRules.ComputeResetCodeHash(FirstPlayerId, Code);

            string spacedCodeHash = CredentialRules.ComputeResetCodeHash(FirstPlayerId, BlankText + Code + BlankText);

            Assert.NotEqual(trimmedCodeHash, spacedCodeHash);
        }

        [Fact]
        public void Compute_ZeroIdentifier_ReturnsADifferentHashThanTheFirstPlayer()
        {
            string firstPlayerHash = CredentialRules.ComputeResetCodeHash(FirstPlayerId, Code);

            string noPlayerHash = CredentialRules.ComputeResetCodeHash(NoPlayerId, Code);

            Assert.NotEqual(firstPlayerHash, noPlayerHash);
        }

        [Fact]
        public void ComputeHash_SamePasswordTwice_ReturnsDifferentHashes()
        {
            string first = CredentialRules.ComputeHash(Password);
            string second = CredentialRules.ComputeHash(Password);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void ComputeHash_AnyPassword_DoesNotContainThePlainPassword()
        {
            string hash = CredentialRules.ComputeHash(Password);

            Assert.DoesNotContain(Password, hash);
        }

        [Fact]
        public void ComputeHash_EmptyPassword_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => CredentialRules.ComputeHash(EmptyText));
        }

        [Fact]
        public void Verify_RightPassword_ReturnsTrue()
        {
            string hash = CredentialRules.ComputeHash(Password);

            bool isValid = CredentialRules.Verify(Password, hash);

            Assert.True(isValid);
        }

        [Fact]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            string hash = CredentialRules.ComputeHash(Password);

            bool isValid = CredentialRules.Verify(OtherPassword, hash);

            Assert.False(isValid);
        }

        [Fact]
        public void Verify_RightPasswordAgainstTheOldUnsaltedHash_ReturnsTrue()
        {
            bool isValid = CredentialRules.Verify(Password, LegacyHashOfPassword);

            Assert.True(isValid);
        }

        [Fact]
        public void Verify_WrongPasswordAgainstTheOldUnsaltedHash_ReturnsFalse()
        {
            bool isValid = CredentialRules.Verify(OtherPassword, LegacyHashOfPassword);

            Assert.False(isValid);
        }

        [Theory]
        [InlineData(MalformedHash)]
        [InlineData(UnknownAlgorithmHash)]
        [InlineData(BrokenBase64Hash)]
        public void Verify_DamagedSaltedHash_ReturnsFalse(string storedHash)
        {
            bool isValid = CredentialRules.Verify(Password, storedHash);

            Assert.False(isValid);
        }

        [Fact]
        public void Verify_EmptyPassword_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => CredentialRules.Verify(EmptyText, LegacyHashOfPassword));
        }

        [Fact]
        public void Verify_EmptyStoredHash_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => CredentialRules.Verify(Password, EmptyText));
        }
    }
}
