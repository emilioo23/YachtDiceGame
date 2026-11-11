using Xunit;
using YachtDice.RoomServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Messages shown when the server rejects an action of a room.
    /// </summary>
    public class RoomResultMessagesTests
    {
        private const int UnknownCode = 99;

        [Fact]
        public void GetMessage_RoomFull_ReturnsTheFullRoomMessage()
        {
            string message = RoomResultMessages.GetMessage(RoomResultCode.RoomFull);

            Assert.Equal(Strings.Lobby_Result_RoomFull, message);
        }

        [Fact]
        public void GetMessage_InvalidAccessCode_ReturnsTheCodeMessage()
        {
            string message = RoomResultMessages.GetMessage(RoomResultCode.InvalidAccessCode);

            Assert.Equal(Strings.Lobby_Result_InvalidAccessCode, message);
        }

        [Fact]
        public void GetMessage_UnknownCode_ReturnsTheUnexpectedError()
        {
            var unknownCode = (RoomResultCode)UnknownCode;

            string message = RoomResultMessages.GetMessage(unknownCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }
    }
}
