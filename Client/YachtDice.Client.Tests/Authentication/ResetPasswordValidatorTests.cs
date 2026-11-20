using System;
using Xunit;
using YachtDice.Client.Logic.Authentication;

namespace YachtDice.Client.Tests.Authentication
{
    /// <summary>
    /// Checks made on the data of the password reset before calling the server.
    /// </summary>
    public class ResetPasswordValidatorTests
    {
        private const string Code = "123456";
        private const string Password = "New456!";
        private const string OtherPassword = "Other789!";
        private const string SixSpaces = "      ";
        private const string ShortCode = "123";

        [Fact]
        public void Validate_CompleteCodeAndMatchingPasswords_ReturnsValid()
        {
            ResetPasswordStatus status = ValidateInput(Code, Password, Password);

            Assert.Equal(ResetPasswordStatus.Valid, status);
        }

        [Theory]
        [InlineData("")]
        [InlineData("12345")]
        [InlineData("1234567")]
        [InlineData("12a456")]
        [InlineData(null)]
        public void Validate_CodeThatIsNotSixDigits_ReturnsIncompleteCode(string code)
        {
            ResetPasswordStatus status = ValidateInput(code, Password, Password);

            Assert.Equal(ResetPasswordStatus.IncompleteCode, status);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validate_BlankPassword_ReturnsEmptyPassword(string password)
        {
            ResetPasswordStatus status = ValidateInput(Code, password, password);

            Assert.Equal(ResetPasswordStatus.EmptyPassword, status);
        }

        [Fact]
        public void Validate_ConfirmationDifferentFromPassword_ReturnsPasswordsDoNotMatch()
        {
            ResetPasswordStatus status = ValidateInput(Code, Password, OtherPassword);

            Assert.Equal(ResetPasswordStatus.PasswordsDoNotMatch, status);
        }

        [Fact]
        public void Validate_CodeOfSpaces_ReturnsIncompleteCode()
        {
            ResetPasswordStatus status = ValidateInput(SixSpaces, Password, Password);

            Assert.Equal(ResetPasswordStatus.IncompleteCode, status);
        }

        [Fact]
        public void Validate_CodeAndPasswordBothWrong_ReportsTheCodeFirst()
        {
            ResetPasswordStatus status = ValidateInput(ShortCode, string.Empty, Password);

            Assert.Equal(ResetPasswordStatus.IncompleteCode, status);
        }

        [Fact]
        public void Validate_BlankPasswordAndDifferentConfirmation_ReportsTheEmptyPassword()
        {
            ResetPasswordStatus status = ValidateInput(Code, string.Empty, Password);

            Assert.Equal(ResetPasswordStatus.EmptyPassword, status);
        }

        [Fact]
        public void Validate_NullConfirmation_ReturnsPasswordsDoNotMatch()
        {
            ResetPasswordStatus status = ValidateInput(Code, Password, null);

            Assert.Equal(ResetPasswordStatus.PasswordsDoNotMatch, status);
        }

        [Fact]
        public void Validate_ConfirmationWithOtherCase_ReturnsPasswordsDoNotMatch()
        {
            ResetPasswordStatus status = ValidateInput(Code, Password, Password.ToUpperInvariant());

            Assert.Equal(ResetPasswordStatus.PasswordsDoNotMatch, status);
        }

        private static ResetPasswordStatus ValidateInput(string code, string newPassword, string confirmation)
        {
            return ResetPasswordValidator.Validate(code, newPassword, confirmation);
        }
    }
}
