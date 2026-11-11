using Xunit;
using YachtDice.AuthenticationServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Messages shown when a login attempt fails.
    /// </summary>
    public class LoginResultMessagesTests
    {
        private const int UnknownCode = 99;

        [Fact]
        public void GetMessage_InvalidCredentials_ReturnsTheCredentialsMessage()
        {
            string message = LoginResultMessages.GetMessage(LoginResultCode.InvalidCredentials);

            Assert.Equal(Strings.Dialogs_InvalidCredentials, message);
        }

        [Fact]
        public void GetMessage_VerificationLocked_ReturnsTheAttemptsMessage()
        {
            string message = LoginResultMessages.GetMessage(LoginResultCode.VerificationLocked);

            Assert.Equal(Strings.Dialogs_TooManyAttempts, message);
        }

        [Fact]
        public void GetMessage_UnknownCode_ReturnsTheUnexpectedError()
        {
            var unknownCode = (LoginResultCode)UnknownCode;

            string message = LoginResultMessages.GetMessage(unknownCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }
    }
}
