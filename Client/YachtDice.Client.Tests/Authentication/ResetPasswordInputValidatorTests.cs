using System;
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
            ResetPasswordInputStatus status = ValidateInput(Code, Password, Password);

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
            ResetPasswordInputStatus status = ValidateInput(code, Password, Password);

            Assert.Equal(ResetPasswordInputStatus.IncompleteCode, status);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validate_BlankPassword_ReturnsEmptyPassword(string password)
        {
            ResetPasswordInputStatus status = ValidateInput(Code, password, password);

            Assert.Equal(ResetPasswordInputStatus.EmptyPassword, status);
        }

        [Fact]
        public void Validate_ConfirmationDifferentFromPassword_ReturnsPasswordsDoNotMatch()
        {
            ResetPasswordInputStatus status = ValidateInput(Code, Password, OtherPassword);

            Assert.Equal(ResetPasswordInputStatus.PasswordsDoNotMatch, status);
        }

        [Fact]
        public void Validate_CodeOfSpaces_ReturnsIncompleteCode()
        {
            ResetPasswordInputStatus status = ValidateInput(SixSpaces, Password, Password);

            Assert.Equal(ResetPasswordInputStatus.IncompleteCode, status);
        }

        [Fact]
        public void Validate_CodeAndPasswordBothWrong_ReportsTheCodeFirst()
        {
            ResetPasswordInputStatus status = ValidateInput(ShortCode, string.Empty, Password);

            Assert.Equal(ResetPasswordInputStatus.IncompleteCode, status);
        }

        [Fact]
        public void Validate_BlankPasswordAndDifferentConfirmation_ReportsTheEmptyPassword()
        {
            ResetPasswordInputStatus status = ValidateInput(Code, string.Empty, Password);

            Assert.Equal(ResetPasswordInputStatus.EmptyPassword, status);
        }

        [Fact]
        public void Validate_NullConfirmation_ReturnsPasswordsDoNotMatch()
        {
            ResetPasswordInputStatus status = ValidateInput(Code, Password, null);

            Assert.Equal(ResetPasswordInputStatus.PasswordsDoNotMatch, status);
        }

        [Fact]
        public void Validate_ConfirmationWithOtherCase_ReturnsPasswordsDoNotMatch()
        {
            ResetPasswordInputStatus status = ValidateInput(Code, Password, Password.ToUpperInvariant());

            Assert.Equal(ResetPasswordInputStatus.PasswordsDoNotMatch, status);
        }

        [Fact]
        public void Validate_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ResetPasswordInputValidator.Validate(null));
        }

        private static ResetPasswordInputStatus ValidateInput(string code, string newPassword, string confirmation)
        {
            var input = new ResetPasswordInput(code, newPassword, confirmation);

            return ResetPasswordInputValidator.Validate(input);
        }
    }
}
