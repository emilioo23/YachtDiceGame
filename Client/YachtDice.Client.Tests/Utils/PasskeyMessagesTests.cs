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
            string message = PasskeyMessages.GetStartMessage(PasskeyStartResultCode.TooManyPasskeys);

            Assert.Equal(Strings.Passkey_ErrorTooMany, message);
        }

        [Fact]
        public void GetStartMessage_Busy_ReturnsPasskeyErrorBusy()
        {
            string message = PasskeyMessages.GetStartMessage(PasskeyStartResultCode.Busy);

            Assert.Equal(Strings.Passkey_ErrorBusy, message);
        }

        [Fact]
        public void GetStartMessage_PlayerNotFound_ReturnsDialogsUnexpectedError()
        {
            string message = PasskeyMessages.GetStartMessage(PasskeyStartResultCode.PlayerNotFound);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetRegistrationMessage_Registered_ReturnsPasskeyRegistered()
        {
            string message = PasskeyMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.Registered);

            Assert.Equal(Strings.Passkey_Registered, message);
        }

        [Fact]
        public void GetRegistrationMessage_InvalidResponse_ReturnsPasskeyErrorInvalid()
        {
            string message = PasskeyMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.InvalidResponse);

            Assert.Equal(Strings.Passkey_ErrorInvalid, message);
        }

        [Fact]
        public void GetRegistrationMessage_ChallengeExpired_ReturnsPasskeyErrorExpired()
        {
            string message = PasskeyMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.ChallengeExpired);

            Assert.Equal(Strings.Passkey_ErrorExpired, message);
        }

        [Fact]
        public void GetRegistrationMessage_TooManyPasskeys_ReturnsPasskeyErrorTooMany()
        {
            string message = PasskeyMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.TooManyPasskeys);

            Assert.Equal(Strings.Passkey_ErrorTooMany, message);
        }

        [Fact]
        public void GetRegistrationMessage_AlreadyRegistered_ReturnsPasskeyErrorAlreadyRegistered()
        {
            string message = PasskeyMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.AlreadyRegistered);

            Assert.Equal(Strings.Passkey_ErrorAlreadyRegistered, message);
        }

        [Fact]
        public void GetRegistrationMessage_PlayerNotFound_ReturnsDialogsUnexpectedError()
        {
            string message = PasskeyMessages.GetRegistrationMessage(PasskeyRegistrationResultCode.PlayerNotFound);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetLoginMessage_InvalidPasskey_ReturnsPasskeyErrorInvalid()
        {
            string message = PasskeyMessages.GetLoginMessage(LoginResultCode.InvalidPasskey);

            Assert.Equal(Strings.Passkey_ErrorInvalid, message);
        }

        [Fact]
        public void GetLoginMessage_TemporarilyLocked_ReturnsAuthErrorTemporarilyLocked()
        {
            string message = PasskeyMessages.GetLoginMessage(LoginResultCode.TemporarilyLocked);

            Assert.Equal(Strings.Auth_ErrorTemporarilyLocked, message);
        }

        [Fact]
        public void GetLoginMessage_LockedForTheDay_ReturnsAuthErrorLockedForTheDay()
        {
            string message = PasskeyMessages.GetLoginMessage(LoginResultCode.LockedForTheDay);

            Assert.Equal(Strings.Auth_ErrorLockedForTheDay, message);
        }

        [Fact]
        public void GetLoginMessage_SuspendedByReports_ReturnsAuthErrorSuspendedByReports()
        {
            string message = PasskeyMessages.GetLoginMessage(LoginResultCode.SuspendedByReports);

            Assert.Equal(Strings.Auth_ErrorSuspendedByReports, message);
        }

        [Fact]
        public void GetLoginMessage_InvalidCredentials_ReturnsDialogsUnexpectedError()
        {
            string message = PasskeyMessages.GetLoginMessage(LoginResultCode.InvalidCredentials);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }
    }
}
