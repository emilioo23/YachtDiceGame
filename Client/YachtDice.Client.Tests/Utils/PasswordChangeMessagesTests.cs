using Xunit;
using YachtDice.Resources;
using YachtDice.TwoFactorManagementServiceReference;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Messages that tell the player how the change of password ended.
    /// </summary>
    public class PasswordChangeMessagesTests
    {
        private const int UnknownResultCode = 99;

        [Fact]
        public void GetMessage_Success_ReturnsTheChangedMessage()
        {
            string message = PasswordChangeMessages.GetMessage(PasswordChangeResultCode.Success);

            Assert.Equal(Strings.Settings_Password_Changed, message);
        }

        [Fact]
        public void GetMessage_WrongCurrentPassword_ReturnsTheWrongPasswordMessage()
        {
            string message = PasswordChangeMessages.GetMessage(PasswordChangeResultCode.WrongCurrentPassword);

            Assert.Equal(Strings.Settings_Password_WrongCurrent, message);
        }

        [Fact]
        public void GetMessage_InvalidNewPassword_ReturnsTheWeakPasswordMessage()
        {
            string message = PasswordChangeMessages.GetMessage(PasswordChangeResultCode.InvalidNewPassword);

            Assert.Equal(Strings.Auth_ErrorWeakPassword, message);
        }

        [Fact]
        public void GetMessage_PasswordUsedBefore_ReturnsThePasswordUsedBeforeMessage()
        {
            string message = PasswordChangeMessages.GetMessage(PasswordChangeResultCode.PasswordUsedBefore);

            Assert.Equal(Strings.ResetPassword_ErrorPasswordUsedBefore, message);
        }

        [Fact]
        public void GetMessage_TooManyAttempts_ReturnsTheLockedMessage()
        {
            string message = PasswordChangeMessages.GetMessage(PasswordChangeResultCode.TooManyAttempts);

            Assert.Equal(Strings.Auth_ErrorTemporarilyLocked, message);
        }

        [Fact]
        public void GetMessage_PlayerNotFound_ReturnsTheUnexpectedErrorMessage()
        {
            string message = PasswordChangeMessages.GetMessage(PasswordChangeResultCode.PlayerNotFound);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetMessage_UnknownResult_ReturnsTheUnexpectedErrorMessage()
        {
            string message = PasswordChangeMessages.GetMessage((PasswordChangeResultCode)UnknownResultCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }
    }
}
