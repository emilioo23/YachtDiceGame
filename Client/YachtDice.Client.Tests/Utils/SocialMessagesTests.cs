using System;
using Xunit;
using YachtDice.FriendServiceReference;
using YachtDice.Resources;
using YachtDice.RoomServiceReference;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Texts of the results of the operations with friends, invitations, reports and kicks.
    /// </summary>
    public class SocialMessagesTests
    {
        private const int UnknownCode = 99;
        private const string FriendName = "Beto";
        private const string OtherFriendName = "Ana";
        private const string HourFormat = "t";
        private const string RoomCode = "ABC123";
        private const int UnknownResultCode = 99;
        private const string OwnName = "gabriel";
        private const string OtherName = "emilio";
        private static readonly DateTime SampleDate = new DateTime(2026, 11, 16, 18, 30, 0, DateTimeKind.Utc);

        [Fact]
        public void GetMessage_SelfRequest_ReturnsTheSelfRequestMessage()
        {
            string message = SocialMessages.GetMessage(FriendRequestResultCode.SelfRequest);

            Assert.Equal(Strings.Friends_Result_SelfRequest, message);
        }

        [Fact]
        public void GetMessage_AlreadyFriends_ReturnsTheFriendsMessage()
        {
            string message = SocialMessages.GetMessage(FriendRequestResultCode.AlreadyFriends);

            Assert.Equal(Strings.Friends_Result_AlreadyFriends, message);
        }

        [Fact]
        public void GetMessage_UnknownCode_ReturnsTheUnexpectedError()
        {
            var unknownCode = (FriendRequestResultCode)UnknownCode;

            string message = SocialMessages.GetMessage(unknownCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetMessage_RequestSent_ReturnsTheMessageOfThatResult()
        {
            string message = SocialMessages.GetMessage(FriendRequestResultCode.RequestSent);

            Assert.Equal(Strings.Friends_Result_RequestSent, message);
        }

        [Fact]
        public void GetMessage_RequestAlreadyReceived_ReturnsTheMessageOfThatResult()
        {
            string message = SocialMessages.GetMessage(FriendRequestResultCode.RequestAlreadyReceived);

            Assert.Equal(Strings.Friends_Result_RequestAlreadyReceived, message);
        }

        [Fact]
        public void GetMessage_InvalidUsername_ReturnsTheMessageOfThatResult()
        {
            string message = SocialMessages.GetMessage(FriendRequestResultCode.InvalidUsername);

            Assert.Equal(Strings.Friends_Result_InvalidUsername, message);
        }

        [Fact]
        public void GetMessage_UsernameNotFound_ReturnsTheMessageOfThatResult()
        {
            string message = SocialMessages.GetMessage(FriendRequestResultCode.UsernameNotFound);

            Assert.Equal(Strings.Friends_Result_UsernameNotFound, message);
        }

        [Fact]
        public void GetMessage_AlreadyRequested_ReturnsTheMessageOfThatResult()
        {
            string message = SocialMessages.GetMessage(FriendRequestResultCode.AlreadyRequested);

            Assert.Equal(Strings.Friends_Result_AlreadyRequested, message);
        }

        [Fact]
        public void GetMessage_Sent_IncludesTheNameOfTheFriend()
        {
            string message = SocialMessages.GetMessage(InvitationResultCode.Sent, FriendName);

            Assert.Equal(string.Format(Strings.Invitations_Result_Sent, FriendName), message);
        }

        [Fact]
        public void GetMessage_NotFriends_ReturnsTheNotFriendsMessage()
        {
            string message = SocialMessages.GetMessage(InvitationResultCode.NotFriends, FriendName);

            Assert.Equal(Strings.Invitations_Result_NotFriends, message);
        }

        [Fact]
        public void GetMessage_InvitationResultMessages_UnknownCode_ReturnsTheUnexpectedError()
        {
            var unknownCode = (InvitationResultCode)UnknownCode;

            string message = SocialMessages.GetMessage(unknownCode, FriendName);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetMessage_NotInRoom_ReturnsTheNotInRoomMessage()
        {
            string message = SocialMessages.GetMessage(InvitationResultCode.NotInRoom, FriendName);

            Assert.Equal(Strings.Invitations_Result_NotInRoom, message);
        }

        [Fact]
        public void GetMessage_FriendOffline_IncludesTheNameOfTheFriend()
        {
            string message = SocialMessages.GetMessage(InvitationResultCode.FriendOffline, FriendName);

            Assert.Equal(string.Format(Strings.Invitations_Result_FriendOffline, FriendName), message);
        }

        [Fact]
        public void GetMessage_SentWithoutName_UsesAnEmptyName()
        {
            string message = SocialMessages.GetMessage(InvitationResultCode.Sent, string.Empty);

            Assert.Equal(string.Format(Strings.Invitations_Result_Sent, string.Empty), message);
        }

        [Fact]
        public void GetMessage_NotFriendsWithAName_IgnoresTheName()
        {
            string message = SocialMessages.GetMessage(InvitationResultCode.NotFriends, OtherFriendName);

            Assert.Equal(Strings.Invitations_Result_NotFriends, message);
        }

        [Fact]
        public void Describe_OpenRoom_ShowsTheRoomAndTheHour()
        {
            var invitation = new ReceivedInvitation { RoomCode = RoomCode, SentAt = SampleDate, IsRoomOpen = true };
            string textExpected = string.Format(Strings.Friends_Invitation_OpenFormat, RoomCode, FormatHour());

            string text = SocialMessages.Describe(invitation);

            Assert.Equal(textExpected, text);
        }

        [Fact]
        public void Describe_ClosedRoom_SaysTheRoomIsClosed()
        {
            var invitation = new ReceivedInvitation { RoomCode = RoomCode, SentAt = SampleDate };
            string textExpected = string.Format(Strings.Friends_Invitation_ClosedFormat, RoomCode, FormatHour());

            string text = SocialMessages.Describe(invitation);

            Assert.Equal(textExpected, text);
        }

        [Fact]
        public void GetMessage_Sent_ReturnsTheThankYouMessage()
        {
            string message = SocialMessages.GetMessage(ReportResultCode.Sent);

            Assert.Equal(Strings.Report_Result_Sent, message);
        }

        [Theory]
        [InlineData(ReportResultCode.PlayerNotFound)]
        [InlineData(ReportResultCode.NoRecentGame)]
        public void GetMessage_PlayerThatCannotBeReported_ReturnsTheNotAllowedMessage(ReportResultCode resultCode)
        {
            string message = SocialMessages.GetMessage(resultCode);

            Assert.Equal(Strings.Report_Result_NotAllowed, message);
        }

        [Fact]
        public void GetMessage_AlreadyReported_ReturnsTheAlreadyReportedMessage()
        {
            string message = SocialMessages.GetMessage(ReportResultCode.AlreadyReported);

            Assert.Equal(Strings.Report_Result_AlreadyReported, message);
        }

        [Fact]
        public void GetMessage_CannotReportSelf_ReturnsTheSelfReportMessage()
        {
            string message = SocialMessages.GetMessage(ReportResultCode.CannotReportSelf);

            Assert.Equal(Strings.Report_Result_CannotReportSelf, message);
        }

        [Fact]
        public void GetMessage_InvalidComment_ReturnsTheLongCommentMessage()
        {
            string message = SocialMessages.GetMessage(ReportResultCode.InvalidComment);

            Assert.Equal(Strings.Report_Result_InvalidComment, message);
        }

        [Fact]
        public void GetMessage_UnknownResult_ReturnsTheUnexpectedErrorMessage()
        {
            string message = SocialMessages.GetMessage((ReportResultCode)UnknownResultCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetReasonName_Cheating_ReturnsTheCheatingName()
        {
            string name = SocialMessages.GetReasonName(ReportReason.Cheating);

            Assert.Equal(Strings.Report_Reason_Cheating, name);
        }

        [Fact]
        public void Describe_PlayerWasTheHost_SaysThatThePlayerExpelledTheOther()
        {
            var entry = new KickHistoryEntry { HostName = OwnName, TargetName = OtherName, RoomCode = RoomCode };
            string textExpected = string.Format(Strings.History_KickedByYouFormat, OtherName, RoomCode);

            string text = SocialMessages.Describe(entry, OwnName);

            Assert.Equal(textExpected, text);
        }

        [Fact]
        public void Describe_PlayerWasExpelled_SaysWhoExpelledThePlayer()
        {
            var entry = new KickHistoryEntry { HostName = OtherName, TargetName = OwnName, RoomCode = RoomCode };
            string textExpected = string.Format(Strings.History_KickedYouFormat, OtherName, RoomCode);

            string text = SocialMessages.Describe(entry, OwnName);

            Assert.Equal(textExpected, text);
        }

        private static string FormatHour()
        {
            return SampleDate.ToLocalTime().ToString(HourFormat);
        }
    }
}
