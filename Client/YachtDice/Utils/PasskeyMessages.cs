using YachtDice.Resources;
using YachtDice.TwoFactorManagementServiceReference;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message shown to the player when a passkey cannot be created or used.
    /// </summary>
    public static class PasskeyMessages
    {
        /// <summary>
        /// Gets the message that matches the answer of the server when the player asks to create a passkey.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetStartMessage(PasskeyStartResultCode resultCode)
        {
            string message;

            switch (resultCode)
            {
                case PasskeyStartResultCode.TooManyPasskeys:
                    message = Strings.Passkey_ErrorTooMany;
                    break;
                case PasskeyStartResultCode.Busy:
                    message = Strings.Passkey_ErrorBusy;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

        /// <summary>
        /// Gets the message that matches the answer of the server when the player sends the new passkey.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetRegistrationMessage(PasskeyRegistrationResultCode resultCode)
        {
            string message;

            switch (resultCode)
            {
                case PasskeyRegistrationResultCode.Registered:
                    message = Strings.Passkey_Registered;
                    break;
                case PasskeyRegistrationResultCode.InvalidResponse:
                    message = Strings.Passkey_ErrorInvalid;
                    break;
                case PasskeyRegistrationResultCode.ChallengeExpired:
                    message = Strings.Passkey_ErrorExpired;
                    break;
                case PasskeyRegistrationResultCode.TooManyPasskeys:
                    message = Strings.Passkey_ErrorTooMany;
                    break;
                case PasskeyRegistrationResultCode.AlreadyRegistered:
                    message = Strings.Passkey_ErrorAlreadyRegistered;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

        /// <summary>
        /// Gets the message that matches the answer of the server when the player removes a passkey.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetRemovalMessage(PasskeyRemovalResultCode resultCode)
        {
            string message;

            switch (resultCode)
            {
                case PasskeyRemovalResultCode.Removed:
                    message = Strings.Passkey_Removed;
                    break;
                case PasskeyRemovalResultCode.InvalidPassword:
                    message = Strings.Dialogs_InvalidCredentials;
                    break;
                case PasskeyRemovalResultCode.PasskeyNotFound:
                    message = Strings.Passkey_ErrorNotFound;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

        /// <summary>
        /// Gets the message that matches the outcome of the login with a passkey.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetLoginMessage(LoginResultCode resultCode)
        {
            string message;

            switch (resultCode)
            {
                case LoginResultCode.InvalidPasskey:
                    message = Strings.Passkey_ErrorInvalid;
                    break;
                case LoginResultCode.TemporarilyLocked:
                    message = Strings.Auth_ErrorTemporarilyLocked;
                    break;
                case LoginResultCode.LockedForTheDay:
                    message = Strings.Auth_ErrorLockedForTheDay;
                    break;
                case LoginResultCode.SuspendedByReports:
                    message = Strings.Auth_ErrorSuspendedByReports;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }
    }
}
