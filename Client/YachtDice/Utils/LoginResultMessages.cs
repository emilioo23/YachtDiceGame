using YachtDice.AuthenticationServiceReference;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message shown to the player when the login does not succeed. The message says whether the
    /// email or the password is the one that failed.
    /// </summary>
    public static class LoginResultMessages
    {
        /// <summary>
        /// Gets the localized message that matches the outcome of the login.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(LoginResultCode resultCode)
        {
            string message;

            switch (resultCode)
            {
                case LoginResultCode.MissingRequiredField:
                    message = Strings.Auth_ErrorRequiredFields;
                    break;
                case LoginResultCode.InvalidCredentials:
                    message = Strings.Dialogs_InvalidCredentials;
                    break;
                case LoginResultCode.InvalidEmail:
                    message = Strings.Auth_ErrorInvalidEmail;
                    break;
                case LoginResultCode.EmailNotRegistered:
                    message = Strings.Dialogs_EmailNotRegistered;
                    break;
                case LoginResultCode.InvalidPassword:
                    message = Strings.Dialogs_InvalidPassword;
                    break;
                case LoginResultCode.VerificationLocked:
                    message = Strings.Dialogs_TooManyAttempts;
                    break;
                case LoginResultCode.CodeSendFailed:
                    message = Strings.TwoFactor_ErrorSendFailed;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }
    }
}
