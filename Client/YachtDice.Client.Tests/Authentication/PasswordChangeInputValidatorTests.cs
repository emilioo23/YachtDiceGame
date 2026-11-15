using Xunit;
using YachtDice.Client.Logic.Authentication;

namespace YachtDice.Client.Tests.Authentication
{
    /// <summary>
    /// Checks made on the three passwords of the password change before calling the server.
    /// </summary>
    public class PasswordChangeInputValidatorTests
    {
        private const string CurrentPassword = "Old123!";
        private const string NewPassword = "New456!";
        private const string OtherPassword = "Other789!";
        private const string Spaces = "      ";

        [Fact]
        public void Validate_ThreePasswordsWithMatchingConfirmation_ReturnsValid()
        {
            PasswordChangeInputStatus status =
                PasswordChangeInputValidator.Validate(CurrentPassword, NewPassword, NewPassword);

            Assert.Equal(PasswordChangeInputStatus.Valid, status);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyCurrentPassword_ReturnsEmptyCurrentPassword(string currentPassword)
        {
            PasswordChangeInputStatus status =
                PasswordChangeInputValidator.Validate(currentPassword, NewPassword, NewPassword);

            Assert.Equal(PasswordChangeInputStatus.EmptyCurrentPassword, status);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("      ")]
        public void Validate_EmptyNewPassword_ReturnsEmptyNewPassword(string newPassword)
        {
            PasswordChangeInputStatus status =
                PasswordChangeInputValidator.Validate(CurrentPassword, newPassword, newPassword);

            Assert.Equal(PasswordChangeInputStatus.EmptyNewPassword, status);
        }

        [Fact]
        public void Validate_ConfirmationDifferentFromTheNewPassword_ReturnsPasswordsDoNotMatch()
        {
            PasswordChangeInputStatus status =
                PasswordChangeInputValidator.Validate(CurrentPassword, NewPassword, OtherPassword);

            Assert.Equal(PasswordChangeInputStatus.PasswordsDoNotMatch, status);
        }

        [Fact]
        public void Validate_CurrentPasswordOfOnlySpaces_ReturnsValid()
        {
            PasswordChangeInputStatus status =
                PasswordChangeInputValidator.Validate(Spaces, NewPassword, NewPassword);

            Assert.Equal(PasswordChangeInputStatus.Valid, status);
        }
    }
}
