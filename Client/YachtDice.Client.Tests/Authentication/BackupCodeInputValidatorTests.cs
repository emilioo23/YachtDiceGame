using Xunit;
using YachtDice.Client.Logic.Authentication;

namespace YachtDice.Client.Tests.Authentication
{
    /// <summary>
    /// Shape of the backup code that the player writes instead of the code sent by email.
    /// </summary>
    public class BackupCodeInputValidatorTests
    {
        private const string ValidCode = "12345678";
        private const string ShortCode = "123456";
        private const string CodeWithLetters = "1234abcd";

        [Fact]
        public void IsValid_EightDigits_ReturnsTrue()
        {
            bool isValid = BackupCodeInputValidator.IsValid(ValidCode);

            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_FewerDigits_ReturnsFalse()
        {
            bool isValid = BackupCodeInputValidator.IsValid(ShortCode);

            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_Letters_ReturnsFalse()
        {
            bool isValid = BackupCodeInputValidator.IsValid(CodeWithLetters);

            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_Null_ReturnsFalse()
        {
            bool isValid = BackupCodeInputValidator.IsValid(null);

            Assert.False(isValid);
        }
    }
}
