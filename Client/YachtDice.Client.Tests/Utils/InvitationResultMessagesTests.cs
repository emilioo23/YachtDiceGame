using Xunit;
using YachtDice.FriendServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Messages shown after inviting a friend to a room.
    /// </summary>
    public class InvitationResultMessagesTests
    {
        private const int UnknownCode = 99;
        private const string FriendName = "Beto";
        private const string OtherFriendName = "Ana";


        [Fact]
        public void GetMessage_Sent_IncludesTheNameOfTheFriend()
        {
            string message = InvitationResultMessages.GetMessage(InvitationResultCode.Sent, FriendName);

            Assert.Equal(string.Format(Strings.Invitations_Result_Sent, FriendName), message);
        }

        [Fact]
        public void GetMessage_NotFriends_ReturnsTheNotFriendsMessage()
        {
            string message = InvitationResultMessages.GetMessage(InvitationResultCode.NotFriends, FriendName);

            Assert.Equal(Strings.Invitations_Result_NotFriends, message);
        }

        [Fact]
        public void GetMessage_UnknownCode_ReturnsTheUnexpectedError()
        {
            var unknownCode = (InvitationResultCode)UnknownCode;

            string message = InvitationResultMessages.GetMessage(unknownCode, FriendName);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetMessage_NotInRoom_ReturnsTheNotInRoomMessage()
        {
            string message = InvitationResultMessages.GetMessage(InvitationResultCode.NotInRoom, FriendName);

            Assert.Equal(Strings.Invitations_Result_NotInRoom, message);
        }

        [Fact]
        public void GetMessage_FriendOffline_IncludesTheNameOfTheFriend()
        {
            string message = InvitationResultMessages.GetMessage(InvitationResultCode.FriendOffline, FriendName);

            Assert.Equal(string.Format(Strings.Invitations_Result_FriendOffline, FriendName), message);
        }

        [Fact]
        public void GetMessage_SentWithoutName_UsesAnEmptyName()
        {
            string message = InvitationResultMessages.GetMessage(InvitationResultCode.Sent, string.Empty);

            Assert.Equal(string.Format(Strings.Invitations_Result_Sent, string.Empty), message);
        }

        [Fact]
        public void GetMessage_NotFriendsWithAName_IgnoresTheName()
        {
            string message = InvitationResultMessages.GetMessage(InvitationResultCode.NotFriends, OtherFriendName);

            Assert.Equal(Strings.Invitations_Result_NotFriends, message);
        }
    }
}
