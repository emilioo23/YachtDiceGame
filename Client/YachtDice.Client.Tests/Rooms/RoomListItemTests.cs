using Xunit;
using YachtDice.Client.Logic.Rooms;

namespace YachtDice.Client.Tests.Rooms
{
    /// <summary>
    /// Rules to compare two rows of the list of rooms.
    /// </summary>
    public class RoomListItemTests
    {
        [Fact]
        public void HasSameContentAs_EqualRowsWithDifferentTypedCodes_ReturnsTrue()
        {
            RoomListItem first = CreateRoom();
            RoomListItem second = CreateRoom();
            first.AccessCode = "AAAAAA";
            second.AccessCode = "BBBBBB";

            Assert.True(first.HasSameContentAs(second));
        }

        [Fact]
        public void HasSameContentAs_DifferentRoomIdentifier_ReturnsFalse()
        {
            RoomListItem second = CreateRoom();
            second.RoomId = "other";

            Assert.False(CreateRoom().HasSameContentAs(second));
        }

        [Fact]
        public void HasSameContentAs_DifferentDetails_ReturnsFalse()
        {
            RoomListItem second = CreateRoom();
            second.Details = "3/4";

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
                RoomId = "room",
                Title = "Sala de Emilio",
                Details = "2/4",
                PrivacyText = "Publica",
                HasSpace = true
            };
        }
    }
}
