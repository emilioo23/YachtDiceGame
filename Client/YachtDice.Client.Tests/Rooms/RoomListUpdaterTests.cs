using System.Collections.Generic;
using Xunit;
using YachtDice.Client.Logic.Rooms;

namespace YachtDice.Client.Tests.Rooms
{
    /// <summary>
    /// Rules to replace the list of rooms without losing what the player was doing in it.
    /// </summary>
    public class RoomListUpdaterTests
    {
        private const string FirstRoomId = "room-1";
        private const string SecondRoomId = "room-2";
        private const string TypedCode = "ABC123";
        private const string NoCode = "";

        [Fact]
        public void HasSameContent_SameRooms_ReturnsTrue()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, 1), CreateRoom(SecondRoomId, 2) };
            var newRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, 1), CreateRoom(SecondRoomId, 2) };

            Assert.True(RoomListUpdater.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_PlayerTypedACodeInAnOldRow_IgnoresTheCode()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, 1) };
            var newRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, 1) };
            oldRooms[0].AccessCode = TypedCode;

            Assert.True(RoomListUpdater.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_ARoomChangedItsPlayers_ReturnsFalse()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, 1) };
            var newRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, 2) };

            Assert.False(RoomListUpdater.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_DifferentNumberOfRooms_ReturnsFalse()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, 1) };
            var newRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, 1), CreateRoom(SecondRoomId, 1) };

            Assert.False(RoomListUpdater.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_SameRoomsInAnotherOrder_ReturnsFalse()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, 1), CreateRoom(SecondRoomId, 1) };
            var newRooms = new List<RoomListItem> { CreateRoom(SecondRoomId, 1), CreateRoom(FirstRoomId, 1) };

            Assert.False(RoomListUpdater.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_BothListsEmpty_ReturnsTrue()
        {
            Assert.True(RoomListUpdater.HasSameContent(new List<RoomListItem>(), new List<RoomListItem>()));
        }

        [Fact]
        public void KeepTypedCodes_RoomStillInTheList_CopiesTheTypedCode()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, 1) };
            var newRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, 2) };
            oldRooms[0].AccessCode = TypedCode;

            RoomListUpdater.KeepTypedCodes(oldRooms, newRooms);

            Assert.Equal(TypedCode, newRooms[0].AccessCode);
        }

        [Fact]
        public void KeepTypedCodes_NewRoomInTheList_LeavesItsCodeEmpty()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, 1) };
            var newRooms = new List<RoomListItem> { CreateRoom(SecondRoomId, 1) };
            oldRooms[0].AccessCode = TypedCode;

            RoomListUpdater.KeepTypedCodes(oldRooms, newRooms);

            Assert.Equal(NoCode, newRooms[0].AccessCode);
        }

        private static RoomListItem CreateRoom(string roomId, int playerCount)
        {
            return new RoomListItem
            {
                RoomId = roomId,
                Title = roomId,
                Details = playerCount.ToString(),
                PrivacyText = NoCode,
                HasSpace = true
            };
        }
    }
}
