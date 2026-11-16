using Xunit;
using YachtDice.Resources;
using YachtDice.TwoFactorManagementServiceReference;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Messages that tell the player how the creation of the backup codes ended.
    /// </summary>
    public class BackupCodesMessagesTests
    {
        private const int UnknownResultCode = 99;

        [Fact]
        public void GetMessage_Created_ReturnsTheKeepThemSafeMessage()
        {
            string message = BackupCodesMessages.GetMessage(BackupCodesResultCode.Created);

            Assert.Equal(Strings.Settings_Security_BackupCodesCreated, message);
        }

        [Fact]
        public void GetMessage_InvalidPassword_ReturnsTheInvalidCredentialsMessage()
        {
            string message = BackupCodesMessages.GetMessage(BackupCodesResultCode.InvalidPassword);

            Assert.Equal(Strings.Dialogs_InvalidCredentials, message);
        }

        [Fact]
        public void GetMessage_TwoFactorDisabled_ReturnsTheTurnOnVerificationMessage()
        {
            string message = BackupCodesMessages.GetMessage(BackupCodesResultCode.TwoFactorDisabled);

            Assert.Equal(Strings.Settings_Security_BackupCodesOff, message);
        }

        [Fact]
        public void GetMessage_PlayerNotFound_ReturnsTheUnavailableMessage()
        {
            string message = BackupCodesMessages.GetMessage(BackupCodesResultCode.PlayerNotFound);

            Assert.Equal(Strings.Settings_Security_TwoFactorUnavailableMessage, message);
        }

        [Fact]
        public void GetMessage_UnknownResult_ReturnsTheUnexpectedErrorMessage()
        {
            string message = BackupCodesMessages.GetMessage((BackupCodesResultCode)UnknownResultCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }
    }
}
