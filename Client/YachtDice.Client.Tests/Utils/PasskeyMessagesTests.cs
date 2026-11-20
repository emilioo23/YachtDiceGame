using Xunit;
using YachtDice.Resources;
using YachtDice.TwoFactorManagementServiceReference;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Messages shown when a passkey cannot be created or used.
    /// </summary>
    public class PasskeyMessagesTests
    {
        [Fact]
        public void GetStartMessage_TooManyPasskeys_ReturnsPasskeyErrorTooMany()
        {
            string messageExpected = Strings.Passkey_ErrorTooMany;

            string messageResult = PasskeyMessages.GetStartMessage(PasskeyStartResultCode.TooManyPasskeys);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetStartMessage_Busy_ReturnsPasskeyErrorBusy()
        {
            string messageExpected = Strings.Passkey_ErrorBusy;

            string messageResult = PasskeyMessages.GetStartMessage(PasskeyStartResultCode.Busy);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetStartMessage_PlayerNotFound_ReturnsDialogsUnexpectedError()
        {
            string messageExpected = Strings.Dialogs_UnexpectedError;

            string messageResult = PasskeyMessages.GetStartMessage(PasskeyStartResultCode.PlayerNotFound);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRegistrationMessage_Registered_ReturnsPasskeyRegistered()
        {
            string messageExpected = Strings.Passkey_Registered;

            string messageResult = PasskeyMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.Registered);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRegistrationMessage_InvalidResponse_ReturnsPasskeyErrorInvalid()
        {
            string messageExpected = Strings.Passkey_ErrorInvalid;

            string messageResult =
                PasskeyMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.InvalidResponse);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRegistrationMessage_ChallengeExpired_ReturnsPasskeyErrorExpired()
        {
            string messageExpected = Strings.Passkey_ErrorExpired;

            string messageResult =
                PasskeyMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.ChallengeExpired);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRegistrationMessage_TooManyPasskeys_ReturnsPasskeyErrorTooMany()
        {
            string messageExpected = Strings.Passkey_ErrorTooMany;

            string messageResult =
                PasskeyMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.TooManyPasskeys);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRegistrationMessage_AlreadyRegistered_ReturnsPasskeyErrorAlreadyRegistered()
        {
            string messageExpected = Strings.Passkey_ErrorAlreadyRegistered;

            string messageResult =
                PasskeyMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.AlreadyRegistered);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRegistrationMessage_PlayerNotFound_ReturnsDialogsUnexpectedError()
        {
            string messageExpected = Strings.Dialogs_UnexpectedError;

            string messageResult = PasskeyMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.PlayerNotFound);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetLoginMessage_InvalidPasskey_ReturnsPasskeyErrorInvalid()
        {
            string messageExpected = Strings.Passkey_ErrorInvalid;

            string messageResult = PasskeyMessages.GetLoginMessage(LoginResultCode.InvalidPasskey);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetLoginMessage_TemporarilyLocked_ReturnsAuthErrorTemporarilyLocked()
        {
            string messageExpected = Strings.Auth_ErrorTemporarilyLocked;

            string messageResult = PasskeyMessages.GetLoginMessage(LoginResultCode.TemporarilyLocked);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetLoginMessage_LockedForTheDay_ReturnsAuthErrorLockedForTheDay()
        {
            string messageExpected = Strings.Auth_ErrorLockedForTheDay;

            string messageResult = PasskeyMessages.GetLoginMessage(LoginResultCode.LockedForTheDay);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetLoginMessage_SuspendedByReports_ReturnsAuthErrorSuspendedByReports()
        {
            string messageExpected = Strings.Auth_ErrorSuspendedByReports;

            string messageResult = PasskeyMessages.GetLoginMessage(LoginResultCode.SuspendedByReports);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetLoginMessage_InvalidCredentials_ReturnsDialogsUnexpectedError()
        {
            string messageExpected = Strings.Dialogs_UnexpectedError;

            string messageResult = PasskeyMessages.GetLoginMessage(LoginResultCode.InvalidCredentials);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRemovalMessage_Removed_ReturnsPasskeyRemoved()
        {
            string messageExpected = Strings.Passkey_Removed;

            string messageResult = PasskeyMessages.GetRemovalMessage(PasskeyRemovalResultCode.Removed);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRemovalMessage_InvalidPassword_ReturnsDialogsInvalidCredentials()
        {
            string messageExpected = Strings.Dialogs_InvalidCredentials;

            string messageResult = PasskeyMessages.GetRemovalMessage(PasskeyRemovalResultCode.InvalidPassword);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRemovalMessage_PasskeyNotFound_ReturnsPasskeyErrorNotFound()
        {
            string messageExpected = Strings.Passkey_ErrorNotFound;

            string messageResult = PasskeyMessages.GetRemovalMessage(PasskeyRemovalResultCode.PasskeyNotFound);

            Assert.Equal(messageExpected, messageResult);
        }

        [Fact]
        public void GetRemovalMessage_PlayerNotFound_ReturnsDialogsUnexpectedError()
        {
            string messageExpected = Strings.Dialogs_UnexpectedError;

            string messageResult = PasskeyMessages.GetRemovalMessage(PasskeyRemovalResultCode.PlayerNotFound);

            Assert.Equal(messageExpected, messageResult);
        }
    }
}
