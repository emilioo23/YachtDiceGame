using Xunit;
using YachtDice.Client.Logic.Rooms;

namespace YachtDice.Client.Tests.Rooms
{
    /// <summary>
    /// Rules to compare two rows of the list of rooms.
    /// </summary>
    public class RoomListItemTests
    {
        private const string FirstTypedCode = "AAAAAA";
        private const string SecondTypedCode = "BBBBBB";
        private const string OtherRoomId = "other";
        private const string OtherDetails = "3/4";
        private const string CreatedRoomId = "room";
        private const string CreatedRoomTitle = "Sala de Emilio";
        private const string CreatedRoomDetails = "2/4";
        private const string CreatedRoomPrivacyText = "Publica";

        [Fact]
        public void HasSameContentAs_EqualRowsWithDifferentTypedCodes_ReturnsTrue()
        {
            RoomListItem first = CreateRoom();
            RoomListItem second = CreateRoom();
            first.AccessCode = FirstTypedCode;
            second.AccessCode = SecondTypedCode;

            Assert.True(first.HasSameContentAs(second));
        }

        [Fact]
        public void HasSameContentAs_DifferentRoomIdentifier_ReturnsFalse()
        {
            RoomListItem second = CreateRoom();
            second.RoomId = OtherRoomId;

            Assert.False(CreateRoom().HasSameContentAs(second));
        }

        [Fact]
        public void HasSameContentAs_DifferentDetails_ReturnsFalse()
        {
            RoomListItem second = CreateRoom();
            second.Details = OtherDetails;

            Assert.False(CreateRoom().HasSameContentAs(second));
        }

        [Fact]
        public void HasSameContentAs_RoomThatFilledUp_ReturnsFalse()
        {
            RoomListItem second = CreateRoom();
            second.HasSpace = false;

            Assert.False(CreateRoom().HasSameContentAs(second));
        }

        [Fact]
        public void AccessCode_NewRow_IsEmpty()
        {
            Assert.Equal(string.Empty, new RoomListItem().AccessCode);
        }

        private static RoomListItem CreateRoom()
        {
            return new RoomListItem
            {
                RoomId = CreatedRoomId,
                Title = CreatedRoomTitle,
                Details = CreatedRoomDetails,
                PrivacyText = CreatedRoomPrivacyText,
                HasSpace = true
            };
        }
    }
}
