using Xunit;
using YachtDice.FriendServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Messages shown after a friend request is sent.
    /// </summary>
    public class FriendRequestMessagesTests
    {
        private const int UnknownCode = 99;

        [Fact]
        public void GetMessage_SelfRequest_ReturnsTheSelfRequestMessage()
        {
            string message = FriendRequestMessages.GetMessage(FriendRequestResultCode.SelfRequest);

            Assert.Equal(Strings.Friends_Result_SelfRequest, message);
        }

        [Fact]
        public void GetMessage_AlreadyFriends_ReturnsTheFriendsMessage()
        {
            string message = FriendRequestMessages.GetMessage(FriendRequestResultCode.AlreadyFriends);

            Assert.Equal(Strings.Friends_Result_AlreadyFriends, message);
        }

        [Fact]
        public void GetMessage_UnknownCode_ReturnsTheUnexpectedError()
        {
            var unknownCode = (FriendRequestResultCode)UnknownCode;

            string message = FriendRequestMessages.GetMessage(unknownCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }
    }
}
