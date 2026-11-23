using System;
using YachtDice.AuthenticationServiceReference;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.TwoFactorManagementServiceReference;

namespace YachtDice.Utils
{
    using AuthenticationLoginResult = YachtDice.AuthenticationServiceReference.LoginResultCode;
    using PasskeyLoginResult = YachtDice.TwoFactorManagementServiceReference.LoginResultCode;

    /// <summary>
    /// Texts that tell the player the result of the operations of the account.
    /// </summary>
    public static class AccountMessages
    {
        private const string ReportedLockReason = "Reported";

        /// <summary>
        /// Gets the localized message that matches the outcome of the login.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(AuthenticationLoginResult resultCode)
        {
            string message;

            switch (resultCode)
            {
                case AuthenticationLoginResult.MissingRequiredField:
                    message = Strings.Auth_ErrorRequiredFields;
                    break;
                case AuthenticationLoginResult.InvalidCredentials:
                    message = Strings.Dialogs_InvalidCredentials;
                    break;
                case AuthenticationLoginResult.InvalidEmail:
                    message = Strings.Auth_ErrorInvalidEmail;
                    break;
                case AuthenticationLoginResult.VerificationLocked:
                    message = Strings.Dialogs_TooManyAttempts;
                    break;
                case AuthenticationLoginResult.CodeSendFailed:
                    message = Strings.TwoFactor_ErrorSendFailed;
                    break;
                case AuthenticationLoginResult.TemporarilyLocked:
                    message = Strings.Auth_ErrorTemporarilyLocked;
                    break;
                case AuthenticationLoginResult.LockedForTheDay:
                    message = Strings.Auth_ErrorLockedForTheDay;
                    break;
                case AuthenticationLoginResult.SuspendedByReports:
                    message = Strings.Auth_ErrorSuspendedByReports;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

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
        public static string GetLoginMessage(PasskeyLoginResult resultCode)
        {
            string message;

            switch (resultCode)
            {
                case PasskeyLoginResult.InvalidPasskey:
                    message = Strings.Passkey_ErrorInvalid;
                    break;
                case PasskeyLoginResult.TemporarilyLocked:
                    message = Strings.Auth_ErrorTemporarilyLocked;
                    break;
                case PasskeyLoginResult.LockedForTheDay:
                    message = Strings.Auth_ErrorLockedForTheDay;
                    break;
                case PasskeyLoginResult.SuspendedByReports:
                    message = Strings.Auth_ErrorSuspendedByReports;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

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

        /// <summary>
        /// Gets the message for a result of the change of username.
        /// </summary>
        /// <param name="resultCode">Result that the server answered.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(UsernameChangeResultCode resultCode)
        {
            string message;

            switch (resultCode)
            {
                case UsernameChangeResultCode.InvalidUsername:
                    message = Strings.Auth_ErrorInvalidUsername;
                    break;
                case UsernameChangeResultCode.OffensiveUsername:
                    message = Strings.Auth_ErrorOffensiveUsername;
                    break;
                case UsernameChangeResultCode.UsernameAlreadyTaken:
                    message = Strings.Dialogs_UsernameTaken;
                    break;
                case UsernameChangeResultCode.ChangedRecently:
                    message = Strings.Profile_UsernameChangedRecently;
                    break;
                default:
                    message = Strings.Profile_UsernameSaveError;
                    break;
            }

            return message;
        }

        /// <summary>
        /// Gets the localized message that matches the outcome of the change.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <param name="requiredLevel">Level that the avatar needs; used by the message of a locked avatar.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(AvatarChangeResultCode resultCode, int requiredLevel)
        {
            string message = Strings.Profile_AvatarSaveError;

            if (resultCode == AvatarChangeResultCode.AvatarLocked)
            {
                message = string.Format(Strings.Profile_AvatarLocked, requiredLevel);
            }

            return message;
        }

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

        /// <summary>
        /// Says when the password changed for the last time, or that it never changed.
        /// </summary>
        /// <param name="summary">Summary that the server sent.</param>
        /// <returns>The sentence in the language of the player.</returns>
        public static string DescribePasswordChange(SecuritySummary summary)
        {
            string text = Strings.Settings_Security_PasswordNeverChanged;

            if (summary.HasPasswordChange)
            {
                string changeDate = FormatDate(summary.LastPasswordChange);
                text = string.Format(Strings.Settings_Security_PasswordChangedFormat, changeDate);
            }

            return text;
        }

        /// <summary>
        /// Says until when the account was locked and why, or nothing if there was no recent lock.
        /// </summary>
        /// <param name="summary">Summary that the server sent.</param>
        /// <returns>The sentence, or an empty text if there was no recent lock.</returns>
        public static string DescribeLock(SecuritySummary summary)
        {
            string text = string.Empty;

            if (summary.HasRecentLock)
            {
                bool isByReports = string.Equals(summary.LockReason, ReportedLockReason, StringComparison.Ordinal);
                string format = isByReports
                    ? Strings.Settings_Security_LockByReportsFormat
                    : Strings.Settings_Security_LockByAttemptsFormat;

                text = string.Format(format, FormatDate(summary.LockedUntil));
            }

            return text;
        }

        private static string FormatDate(DateTime universalDate)
        {
            return universalDate.ToLocalTime().ToString(DisplayFormats.ShortDateTime);
        }
    }
}
