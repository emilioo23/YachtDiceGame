using Xunit;
using YachtDice.Gateway;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Messages shown when a call to the server fails, one for each kind of failure.
    /// </summary>
    public class GatewayErrorMessagesTests
    {
        private const int UnknownCode = 99;

        [Fact]
        public void GetMessage_ServerUnavailable_ReturnsTheServerMessage()
        {
            string message = GatewayErrorMessages.GetMessage(GatewayErrorCode.ServerUnavailable);

            Assert.Equal(Strings.Dialogs_ServerUnavailable, message);
        }

        [Fact]
        public void GetMessage_DatabaseUnavailable_ReturnsTheDatabaseMessage()
        {
            string message = GatewayErrorMessages.GetMessage(GatewayErrorCode.DatabaseUnavailable);

            Assert.Equal(Strings.Dialogs_DatabaseUnavailable, message);
        }

        [Fact]
        public void GetMessage_ValidationFailed_ReturnsTheInvalidDataMessage()
        {
            string message = GatewayErrorMessages.GetMessage(GatewayErrorCode.ValidationFailed);

            Assert.Equal(Strings.Dialogs_InvalidData, message);
        }

        [Fact]
        public void GetMessage_UnexpectedError_ReturnsTheUnexpectedError()
        {
            string message = GatewayErrorMessages.GetMessage(GatewayErrorCode.UnexpectedError);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetMessage_CodeOutsideTheList_ReturnsTheUnexpectedError()
        {
            var unknownCode = (GatewayErrorCode)UnknownCode;

            string message = GatewayErrorMessages.GetMessage(unknownCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }
    }
}
