using System;
using Xunit;
using YachtDice.AuthenticationServiceReference;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.TwoFactorManagementServiceReference;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    using AuthenticationLoginResult = YachtDice.AuthenticationServiceReference.LoginResultCode;
    using PasskeyLoginResult = YachtDice.TwoFactorManagementServiceReference.LoginResultCode;

    /// <summary>
    /// Texts of the results of the operations of the account.
    /// </summary>
    public class AccountMessagesTests
    {
        private const int UnknownCode = 99;
        private const int UnknownResultCode = 99;
        private const int RequiredLevel = 5;
        private const int SecondLevel = 2;
        private const int NoLevel = 0;
        private const string DateFormat = "g";
        private const string ReportedLockReason = "Reported";
        private const string DailyLockReason = "Daily";
        private static readonly DateTime SampleDate = new DateTime(2026, 11, 16, 18, 30, 0, DateTimeKind.Utc);

        [Fact]
        public void GetMessage_InvalidCredentials_ReturnsTheCredentialsMessage()
        {
            string message = AccountMessages.GetMessage(AuthenticationLoginResult.InvalidCredentials);

            Assert.Equal(Strings.Dialogs_InvalidCredentials, message);
        }

        [Fact]
        public void GetMessage_SuspendedByReports_ReturnsTheSuspensionMessage()
        {
            string message = AccountMessages.GetMessage(AuthenticationLoginResult.SuspendedByReports);

            Assert.Equal(Strings.Auth_ErrorSuspendedByReports, message);
        }

        [Fact]
        public void GetMessage_VerificationLocked_ReturnsTheAttemptsMessage()
        {
            string message = AccountMessages.GetMessage(AuthenticationLoginResult.VerificationLocked);

            Assert.Equal(Strings.Dialogs_TooManyAttempts, message);
        }

        [Fact]
        public void GetMessage_UnknownCode_ReturnsTheUnexpectedError()
        {
            var unknownCode = (AuthenticationLoginResult)UnknownCode;

            string message = AccountMessages.GetMessage(unknownCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetMessage_MissingRequiredField_ReturnsTheMessageOfThatResult()
        {
            string message = AccountMessages.GetMessage(AuthenticationLoginResult.MissingRequiredField);

            Assert.Equal(Strings.Auth_ErrorRequiredFields, message);
        }

        [Fact]
        public void GetMessage_InvalidEmail_ReturnsTheMessageOfThatResult()
        {
            string message = AccountMessages.GetMessage(AuthenticationLoginResult.InvalidEmail);

            Assert.Equal(Strings.Auth_ErrorInvalidEmail, message);
        }

        [Fact]
        public void GetMessage_CodeSendFailed_ReturnsTheMessageOfThatResult()
        {
            string message = AccountMessages.GetMessage(AuthenticationLoginResult.CodeSendFailed);

            Assert.Equal(Strings.TwoFactor_ErrorSendFailed, message);
        }

        [Fact]
        public void GetStartMessage_TooManyPasskeys_ReturnsPasskeyErrorTooMany()
        {
            string messageExpected = Strings.Passkey_ErrorTooMany;

            string messageResult = AccountMessages.GetStartMessage(PasskeyStartResultCode.TooManyPasskeys);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetStartMessage_Busy_ReturnsPasskeyErrorBusy()
        {
            string messageExpected = Strings.Passkey_ErrorBusy;

            string messageResult = AccountMessages.GetStartMessage(PasskeyStartResultCode.Busy);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetStartMessage_PlayerNotFound_ReturnsDialogsUnexpectedError()
        {
            string messageExpected = Strings.Dialogs_UnexpectedError;

            string messageResult = AccountMessages.GetStartMessage(PasskeyStartResultCode.PlayerNotFound);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRegistrationMessage_Registered_ReturnsPasskeyRegistered()
        {
            string messageExpected = Strings.Passkey_Registered;

            string messageResult = AccountMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.Registered);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRegistrationMessage_InvalidResponse_ReturnsPasskeyErrorInvalid()
        {
            string messageExpected = Strings.Passkey_ErrorInvalid;

            string messageResult =
                AccountMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.InvalidResponse);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRegistrationMessage_ChallengeExpired_ReturnsPasskeyErrorExpired()
        {
            string messageExpected = Strings.Passkey_ErrorExpired;

            string messageResult =
                AccountMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.ChallengeExpired);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRegistrationMessage_TooManyPasskeys_ReturnsPasskeyErrorTooMany()
        {
            string messageExpected = Strings.Passkey_ErrorTooMany;

            string messageResult =
                AccountMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.TooManyPasskeys);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRegistrationMessage_AlreadyRegistered_ReturnsPasskeyErrorAlreadyRegistered()
        {
            string messageExpected = Strings.Passkey_ErrorAlreadyRegistered;

            string messageResult =
                AccountMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.AlreadyRegistered);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRegistrationMessage_PlayerNotFound_ReturnsDialogsUnexpectedError()
        {
            string messageExpected = Strings.Dialogs_UnexpectedError;

            string messageResult = AccountMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.PlayerNotFound);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetLoginMessage_InvalidPasskey_ReturnsPasskeyErrorInvalid()
        {
            string messageExpected = Strings.Passkey_ErrorInvalid;

            string messageResult = AccountMessages.GetLoginMessage(PasskeyLoginResult.InvalidPasskey);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetLoginMessage_TemporarilyLocked_ReturnsAuthErrorTemporarilyLocked()
        {
            string messageExpected = Strings.Auth_ErrorTemporarilyLocked;

            string messageResult = AccountMessages.GetLoginMessage(PasskeyLoginResult.TemporarilyLocked);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetLoginMessage_LockedForTheDay_ReturnsAuthErrorLockedForTheDay()
        {
            string messageExpected = Strings.Auth_ErrorLockedForTheDay;

            string messageResult = AccountMessages.GetLoginMessage(PasskeyLoginResult.LockedForTheDay);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetLoginMessage_SuspendedByReports_ReturnsAuthErrorSuspendedByReports()
        {
            string messageExpected = Strings.Auth_ErrorSuspendedByReports;

            string messageResult = AccountMessages.GetLoginMessage(PasskeyLoginResult.SuspendedByReports);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetLoginMessage_InvalidCredentials_ReturnsDialogsUnexpectedError()
        {
            string messageExpected = Strings.Dialogs_UnexpectedError;

            string messageResult = AccountMessages.GetLoginMessage(PasskeyLoginResult.InvalidCredentials);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRemovalMessage_Removed_ReturnsPasskeyRemoved()
        {
            string messageExpected = Strings.Passkey_Removed;

            string messageResult = AccountMessages.GetRemovalMessage(PasskeyRemovalResultCode.Removed);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRemovalMessage_InvalidPassword_ReturnsDialogsInvalidCredentials()
        {
            string messageExpected = Strings.Dialogs_InvalidCredentials;

            string messageResult = AccountMessages.GetRemovalMessage(PasskeyRemovalResultCode.InvalidPassword);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRemovalMessage_PasskeyNotFound_ReturnsPasskeyErrorNotFound()
        {
            string messageExpected = Strings.Passkey_ErrorNotFound;

            string messageResult = AccountMessages.GetRemovalMessage(PasskeyRemovalResultCode.PasskeyNotFound);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRemovalMessage_PlayerNotFound_ReturnsDialogsUnexpectedError()
        {
            string messageExpected = Strings.Dialogs_UnexpectedError;

            string messageResult = AccountMessages.GetRemovalMessage(PasskeyRemovalResultCode.PlayerNotFound);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetMessage_Success_ReturnsTheChangedMessage()
        {
            string message = AccountMessages.GetMessage(PasswordChangeResultCode.Success);

            Assert.Equal(Strings.Settings_Password_Changed, message);
        }

        [Fact]
        public void GetMessage_WrongCurrentPassword_ReturnsTheWrongPasswordMessage()
        {
            string message = AccountMessages.GetMessage(PasswordChangeResultCode.WrongCurrentPassword);

            Assert.Equal(Strings.Settings_Password_WrongCurrent, message);
        }

        [Fact]
        public void GetMessage_InvalidNewPassword_ReturnsTheWeakPasswordMessage()
        {
            string message = AccountMessages.GetMessage(PasswordChangeResultCode.InvalidNewPassword);

            Assert.Equal(Strings.Auth_ErrorWeakPassword, message);
        }

        [Fact]
        public void GetMessage_PasswordUsedBefore_ReturnsThePasswordUsedBeforeMessage()
        {
            string message = AccountMessages.GetMessage(PasswordChangeResultCode.PasswordUsedBefore);

            Assert.Equal(Strings.ResetPassword_ErrorPasswordUsedBefore, message);
        }

        [Fact]
        public void GetMessage_TooManyAttempts_ReturnsTheLockedMessage()
        {
            string message = AccountMessages.GetMessage(PasswordChangeResultCode.TooManyAttempts);

            Assert.Equal(Strings.Auth_ErrorTemporarilyLocked, message);
        }

        [Fact]
        public void GetMessage_PlayerNotFound_ReturnsTheUnexpectedErrorMessage()
        {
            string message = AccountMessages.GetMessage(PasswordChangeResultCode.PlayerNotFound);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetMessage_UnknownResult_ReturnsTheUnexpectedErrorMessage()
        {
            string message = AccountMessages.GetMessage((PasswordChangeResultCode)UnknownResultCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetMessage_InvalidUsername_ReturnsTheRulesOfTheUsername()
        {
            string message = AccountMessages.GetMessage(UsernameChangeResultCode.InvalidUsername);

            Assert.Equal(Strings.Auth_ErrorInvalidUsername, message);
        }

        [Fact]
        public void GetMessage_OffensiveUsername_ReturnsTheOffensiveWordMessage()
        {
            string message = AccountMessages.GetMessage(UsernameChangeResultCode.OffensiveUsername);

            Assert.Equal(Strings.Auth_ErrorOffensiveUsername, message);
        }

        [Fact]
        public void GetMessage_UsernameAlreadyTaken_ReturnsTheTakenMessage()
        {
            string message = AccountMessages.GetMessage(UsernameChangeResultCode.UsernameAlreadyTaken);

            Assert.Equal(Strings.Dialogs_UsernameTaken, message);
        }

        [Fact]
        public void GetMessage_PlayerNotFound_ReturnsTheSaveError()
        {
            string message = AccountMessages.GetMessage(UsernameChangeResultCode.PlayerNotFound);

            Assert.Equal(Strings.Profile_UsernameSaveError, message);
        }

        [Fact]
        public void GetMessage_ResultCodeOutsideTheList_ReturnsTheSaveError()
        {
            string message = AccountMessages.GetMessage((UsernameChangeResultCode)UnknownResultCode);

            Assert.Equal(Strings.Profile_UsernameSaveError, message);
        }

        [Fact]
        public void GetMessage_AvatarLocked_IncludesTheRequiredLevel()
        {
            string message = AccountMessages.GetMessage(AvatarChangeResultCode.AvatarLocked, RequiredLevel);

            Assert.Equal(string.Format(Strings.Profile_AvatarLocked, RequiredLevel), message);
        }

        [Fact]
        public void GetMessage_AvatarNotFound_ReturnsTheSaveError()
        {
            string message = AccountMessages.GetMessage(AvatarChangeResultCode.AvatarNotFound, RequiredLevel);

            Assert.Equal(Strings.Profile_AvatarSaveError, message);
        }

        [Fact]
        public void GetMessage_AvatarChangeMessages_PlayerNotFound_ReturnsTheSaveError()
        {
            string message = AccountMessages.GetMessage(AvatarChangeResultCode.PlayerNotFound, RequiredLevel);

            Assert.Equal(Strings.Profile_AvatarSaveError, message);
        }

        [Fact]
        public void GetMessage_AvatarChangeMessages_ResultCodeOutsideTheList_ReturnsTheSaveError()
        {
            string message = AccountMessages.GetMessage((AvatarChangeResultCode)UnknownResultCode, RequiredLevel);

            Assert.Equal(Strings.Profile_AvatarSaveError, message);
        }

        [Fact]
        public void GetMessage_AvatarLockedForTheSecondLevel_IncludesThatLevel()
        {
            string message = AccountMessages.GetMessage(AvatarChangeResultCode.AvatarLocked, SecondLevel);

            Assert.Equal(string.Format(Strings.Profile_AvatarLocked, SecondLevel), message);
        }

        [Fact]
        public void GetMessage_AvatarLockedWithoutLevel_IncludesTheZero()
        {
            string message = AccountMessages.GetMessage(AvatarChangeResultCode.AvatarLocked, NoLevel);

            Assert.Equal(string.Format(Strings.Profile_AvatarLocked, NoLevel), message);
        }

        [Fact]
        public void GetMessage_AvatarNotFoundWithALevel_IgnoresTheLevel()
        {
            string message = AccountMessages.GetMessage(AvatarChangeResultCode.AvatarNotFound, SecondLevel);

            Assert.Equal(Strings.Profile_AvatarSaveError, message);
        }

        [Fact]
        public void GetMessage_Created_ReturnsTheKeepThemSafeMessage()
        {
            string message = AccountMessages.GetMessage(BackupCodesResultCode.Created);

            Assert.Equal(Strings.Settings_Security_BackupCodesCreated, message);
        }

        [Fact]
        public void GetMessage_InvalidPassword_ReturnsTheInvalidCredentialsMessage()
        {
            string message = AccountMessages.GetMessage(BackupCodesResultCode.InvalidPassword);

            Assert.Equal(Strings.Dialogs_InvalidCredentials, message);
        }

        [Fact]
        public void GetMessage_TwoFactorDisabled_ReturnsTheTurnOnVerificationMessage()
        {
            string message = AccountMessages.GetMessage(BackupCodesResultCode.TwoFactorDisabled);

            Assert.Equal(Strings.Settings_Security_BackupCodesOff, message);
        }

        [Fact]
        public void GetMessage_PlayerNotFound_ReturnsTheUnavailableMessage()
        {
            string message = AccountMessages.GetMessage(BackupCodesResultCode.PlayerNotFound);

            Assert.Equal(Strings.Settings_Security_TwoFactorUnavailableMessage, message);
        }

        [Fact]
        public void GetMessage_BackupCodesMessages_UnknownResult_ReturnsTheUnexpectedErrorMessage()
        {
            string message = AccountMessages.GetMessage((BackupCodesResultCode)UnknownResultCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void DescribePasswordChange_NeverChanged_SaysItNeverChanged()
        {
            string text = AccountMessages.DescribePasswordChange(new SecuritySummary());

            Assert.Equal(Strings.Settings_Security_PasswordNeverChanged, text);
        }

        [Fact]
        public void DescribePasswordChange_Changed_ShowsTheDate()
        {
            var summary = new SecuritySummary { HasPasswordChange = true, LastPasswordChange = SampleDate };
            string textExpected =
                string.Format(Strings.Settings_Security_PasswordChangedFormat, FormatDate(SampleDate));

            string text = AccountMessages.DescribePasswordChange(summary);

            Assert.Equal(textExpected, text);
        }

        [Fact]
        public void DescribeLock_NoRecentLock_ReturnsAnEmptyText()
        {
            string text = AccountMessages.DescribeLock(new SecuritySummary());

            Assert.Equal(string.Empty, text);
        }

        [Fact]
        public void DescribeLock_LockByReports_SaysThatPlayersReportedTheAccount()
        {
            var summary = new SecuritySummary
            {
                HasRecentLock = true,
                LockedUntil = SampleDate,
                LockReason = ReportedLockReason
            };
            string textExpected =
                string.Format(Strings.Settings_Security_LockByReportsFormat, FormatDate(SampleDate));

            string text = AccountMessages.DescribeLock(summary);

            Assert.Equal(textExpected, text);
        }

        [Fact]
        public void DescribeLock_LockByWrongPasswords_SaysThatThereWereFailedAttempts()
        {
            var summary = new SecuritySummary
            {
                HasRecentLock = true,
                LockedUntil = SampleDate,
                LockReason = DailyLockReason
            };
            string textExpected =
                string.Format(Strings.Settings_Security_LockByAttemptsFormat, FormatDate(SampleDate));

            string text = AccountMessages.DescribeLock(summary);

            Assert.Equal(textExpected, text);
        }

        private static string FormatDate(DateTime universalDate)
        {
            return universalDate.ToLocalTime().ToString(DateFormat);
        }
    }
}
