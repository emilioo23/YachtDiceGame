using Xunit;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Tests.Security
{
    /// <summary>
    /// Limits and checks of the credentials of the players.
    /// </summary>
    public class CredentialRulesTests
    {
        private const char FillLetter = 'a';
        private const string Digit = "1";
        private const string UniversityDomain = "@uv.mx";
        private const string EmailWithSurroundingSpaces = " gabriel@gmail.com ";

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
        [InlineData("María José")]
        [InlineData("Anne-Marie")]
        [InlineData("O'Brien")]
        [InlineData("Hernández")]
        [InlineData("李明")]
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
        [InlineData("josé")]
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
        [InlineData("contraseña 2026")]
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
    }
}
