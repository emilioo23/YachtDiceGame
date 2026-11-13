using Xunit;
using YachtDice.Client.Logic.Authentication;

namespace YachtDice.Client.Tests.Authentication
{
    /// <summary>
    /// Checks made on the data of the password reset before calling the server.
    /// </summary>
    public class ResetPasswordInputValidatorTests
    {
        private const string Code = "123456";
        private const string Password = "New456!";
        private const string OtherPassword = "Other789!";
        private const string SixSpaces = "      ";
        private const string ShortCode = "123";

        [Fact]
        public void Validate_CompleteCodeAndMatchingPasswords_ReturnsValid()
        {
            ResetPasswordInputStatus status = ResetPasswordInputValidator.Validate(Code, Password, Password);

            Assert.Equal(ResetPasswordInputStatus.Valid, status);
        }

        [Theory]
        [InlineData("")]
        [InlineData("12345")]
        [InlineData("1234567")]
        [InlineData("12a456")]
        [InlineData(null)]
        public void Validate_CodeThatIsNotSixDigits_ReturnsIncompleteCode(string code)
        {
            ResetPasswordInputStatus status = ResetPasswordInputValidator.Validate(code, Password, Password);

            Assert.Equal(ResetPasswordInputStatus.IncompleteCode, status);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validate_BlankPassword_ReturnsEmptyPassword(string password)
        {
            ResetPasswordInputStatus status = ResetPasswordInputValidator.Validate(Code, password, password);

            Assert.Equal(ResetPasswordInputStatus.EmptyPassword, status);
        }

        [Fact]
        public void Validate_ConfirmationDifferentFromPassword_ReturnsPasswordsDoNotMatch()
        {
            ResetPasswordInputStatus status = ResetPasswordInputValidator.Validate(Code, Password, OtherPassword);

            Assert.Equal(ResetPasswordInputStatus.PasswordsDoNotMatch, status);
        }

        [Fact]
        public void Validate_CodeOfSpaces_ReturnsIncompleteCode()
        {
            ResetPasswordInputStatus status = ResetPasswordInputValidator.Validate(SixSpaces, Password, Password);

            Assert.Equal(ResetPasswordInputStatus.IncompleteCode, status);
        }

        [Fact]
        public void Validate_CodeAndPasswordBothWrong_ReportsTheCodeFirst()
        {
            ResetPasswordInputStatus status = ResetPasswordInputValidator.Validate(ShortCode, string.Empty, Password);

            Assert.Equal(ResetPasswordInputStatus.IncompleteCode, status);
        }

        [Fact]
        public void Validate_BlankPasswordAndDifferentConfirmation_ReportsTheEmptyPassword()
        {
            ResetPasswordInputStatus status = ResetPasswordInputValidator.Validate(Code, string.Empty, Password);

            Assert.Equal(ResetPasswordInputStatus.EmptyPassword, status);
        }

        [Fact]
        public void Validate_NullConfirmation_ReturnsPasswordsDoNotMatch()
        {
            ResetPasswordInputStatus status = ResetPasswordInputValidator.Validate(Code, Password, null);

            Assert.Equal(ResetPasswordInputStatus.PasswordsDoNotMatch, status);
        }

        [Fact]
        public void Validate_ConfirmationWithOtherCase_ReturnsPasswordsDoNotMatch()
        {
            ResetPasswordInputStatus status = ResetPasswordInputValidator.Validate(Code, Password, Password.ToUpperInvariant());

            Assert.Equal(ResetPasswordInputStatus.PasswordsDoNotMatch, status);
        }
    }
}
