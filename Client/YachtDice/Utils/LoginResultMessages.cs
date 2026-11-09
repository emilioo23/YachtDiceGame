using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<LoginResultCode, Func<string>> MessageByResult =
            new Dictionary<LoginResultCode, Func<string>>
            {
                { LoginResultCode.MissingRequiredField, () => Strings.Auth_ErrorRequiredFields },
                { LoginResultCode.InvalidCredentials, () => Strings.Dialogs_InvalidCredentials },
                { LoginResultCode.InvalidEmail, () => Strings.Auth_ErrorInvalidEmail },
                { LoginResultCode.EmailNotRegistered, () => Strings.Dialogs_EmailNotRegistered },
                { LoginResultCode.InvalidPassword, () => Strings.Dialogs_InvalidPassword },
                { LoginResultCode.VerificationLocked, () => Strings.Dialogs_TooManyAttempts },
                { LoginResultCode.CodeSendFailed, () => Strings.TwoFactor_ErrorSendFailed }
            };

        /// <summary>
        /// Gets the localized message that matches the outcome of the login.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(LoginResultCode resultCode)
        {
            Func<string> getText;

            if (!MessageByResult.TryGetValue(resultCode, out getText))
            {
                getText = () => Strings.Dialogs_UnexpectedError;
            }

            return getText();
        }
    }
}
