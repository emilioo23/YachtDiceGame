using YachtDice.Resources;
using YachtDice.TwoFactorManagementServiceReference;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message that tells the player how the creation of the backup codes ended.
    /// </summary>
    public static class BackupCodesMessages
    {
        /// <summary>
        /// Gets the message for a result of the creation of the backup codes.
        /// </summary>
        /// <param name="resultCode">Result that the server answered.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(BackupCodesResultCode resultCode)
        {
            string message;

            switch (resultCode)
            {
                case BackupCodesResultCode.Created:
                    message = Strings.Settings_Security_BackupCodesCreated;
                    break;
                case BackupCodesResultCode.InvalidPassword:
                    message = Strings.Dialogs_InvalidCredentials;
                    break;
                case BackupCodesResultCode.TwoFactorDisabled:
                    message = Strings.Settings_Security_BackupCodesOff;
                    break;
                case BackupCodesResultCode.PlayerNotFound:
                    message = Strings.Settings_Security_TwoFactorUnavailableMessage;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }
    }
}
