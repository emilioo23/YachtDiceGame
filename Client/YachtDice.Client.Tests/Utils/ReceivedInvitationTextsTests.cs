using System;
using Xunit;
using YachtDice.FriendServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Sentences that the list of received invitations shows under the name of the friend.
    /// </summary>
    public class ReceivedInvitationTextsTests
    {
        private const string HourFormat = "t";
        private const string RoomCode = "ABC123";

        private static readonly DateTime SampleDate = new DateTime(2026, 11, 16, 18, 30, 0, DateTimeKind.Utc);

        [Fact]
        public void Describe_OpenRoom_ShowsTheRoomAndTheHour()
        {
            var invitation = new ReceivedInvitation { RoomCode = RoomCode, SentAt = SampleDate, IsRoomOpen = true };
            string textExpected = string.Format(Strings.Friends_Invitation_OpenFormat, RoomCode, FormatHour());

            string text = ReceivedInvitationTexts.Describe(invitation);

            Assert.Equal(textExpected, text);
        }

        [Fact]
        public void Describe_ClosedRoom_SaysTheRoomIsClosed()
        {
            var invitation = new ReceivedInvitation { RoomCode = RoomCode, SentAt = SampleDate };
            string textExpected = string.Format(Strings.Friends_Invitation_ClosedFormat, RoomCode, FormatHour());

            string text = ReceivedInvitationTexts.Describe(invitation);

            Assert.Equal(textExpected, text);
        }

        private static string FormatHour()
        {
            return SampleDate.ToLocalTime().ToString(HourFormat);
        }
    }
}
