using YachtDice.Resources;
using YachtDice.TwoFactorManagementServiceReference;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message that tells the player how the change of password ended.
    /// </summary>
    public static class PasswordChangeMessages
    {
        /// <summary>
        /// Gets the message for a result of the change of password.
        /// </summary>
        /// <param name="resultCode">Result that the server answered.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(PasswordChangeResultCode resultCode)
        {
            string message;

            switch (resultCode)
            {
                case PasswordChangeResultCode.Success:
                    message = Strings.Settings_Password_Changed;
                    break;
                case PasswordChangeResultCode.WrongCurrentPassword:
                    message = Strings.Settings_Password_WrongCurrent;
                    break;
                case PasswordChangeResultCode.InvalidNewPassword:
                    message = Strings.Auth_ErrorWeakPassword;
                    break;
                case PasswordChangeResultCode.PasswordUsedBefore:
                    message = Strings.ResetPassword_ErrorPasswordUsedBefore;
                    break;
                case PasswordChangeResultCode.TooManyAttempts:
                    message = Strings.Auth_ErrorTemporarilyLocked;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }
    }
}
