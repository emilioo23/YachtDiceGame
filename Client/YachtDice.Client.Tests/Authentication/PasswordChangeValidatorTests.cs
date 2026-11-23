using Xunit;
using YachtDice.Client.Logic.Authentication;

namespace YachtDice.Client.Tests.Authentication
{
    /// <summary>
    /// Checks made on the three passwords of the password change before calling the server.
    /// </summary>
    public class PasswordChangeValidatorTests
    {
        private const string CurrentPassword = "Old123!";
        private const string NewPassword = "New456!";
        private const string OtherPassword = "Other789!";
        private const string Spaces = "      ";

        [Fact]
        public void Validate_ThreePasswordsWithMatchingConfirmation_ReturnsValid()
        {
            PasswordChangeStatus status =
                PasswordChangeValidator.Validate(CurrentPassword, NewPassword, NewPassword);

            Assert.Equal(PasswordChangeStatus.Valid, status);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyCurrentPassword_ReturnsEmptyCurrentPassword(string currentPassword)
        {
            PasswordChangeStatus status =
                PasswordChangeValidator.Validate(currentPassword, NewPassword, NewPassword);

            Assert.Equal(PasswordChangeStatus.EmptyCurrentPassword, status);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("      ")]
        public void Validate_EmptyNewPassword_ReturnsEmptyNewPassword(string newPassword)
        {
            PasswordChangeStatus status =
                PasswordChangeValidator.Validate(CurrentPassword, newPassword, newPassword);

            Assert.Equal(PasswordChangeStatus.EmptyNewPassword, status);
        }

        [Fact]
        public void Validate_ConfirmationDifferentFromTheNewPassword_ReturnsPasswordsDoNotMatch()
        {
            PasswordChangeStatus status =
                PasswordChangeValidator.Validate(CurrentPassword, NewPassword, OtherPassword);

            Assert.Equal(PasswordChangeStatus.PasswordsDoNotMatch, status);
        }

        [Fact]
        public void Validate_CurrentPasswordOfOnlySpaces_ReturnsValid()
        {
            PasswordChangeStatus status =
                PasswordChangeValidator.Validate(Spaces, NewPassword, NewPassword);

            Assert.Equal(PasswordChangeStatus.Valid, status);
        }
    }
}
