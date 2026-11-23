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
        private const int OnePlayer = 1;
        private const int TwoPlayers = 2;

        [Fact]
        public void HasSameContent_SameRooms_ReturnsTrue()
        {
            var oldRooms = new List<RoomListItem>
            {
                CreateRoom(FirstRoomId, OnePlayer),
                CreateRoom(SecondRoomId, TwoPlayers)
            };
            var newRooms = new List<RoomListItem>
            {
                CreateRoom(FirstRoomId, OnePlayer),
                CreateRoom(SecondRoomId, TwoPlayers)
            };

            Assert.True(RoomListUpdater.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_PlayerTypedACodeInAnOldRow_IgnoresTheCode()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, OnePlayer) };
            var newRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, OnePlayer) };
            oldRooms[0].AccessCode = TypedCode;

            Assert.True(RoomListUpdater.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_ARoomChangedItsPlayers_ReturnsFalse()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, OnePlayer) };
            var newRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, TwoPlayers) };

            Assert.False(RoomListUpdater.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_DifferentNumberOfRooms_ReturnsFalse()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, OnePlayer) };
            var newRooms = new List<RoomListItem>
            {
                CreateRoom(FirstRoomId, OnePlayer),
                CreateRoom(SecondRoomId, OnePlayer)
            };

            Assert.False(RoomListUpdater.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_SameRoomsInAnotherOrder_ReturnsFalse()
        {
            var oldRooms = new List<RoomListItem>
            {
                CreateRoom(FirstRoomId, OnePlayer),
                CreateRoom(SecondRoomId, OnePlayer)
            };
            var newRooms = new List<RoomListItem>
            {
                CreateRoom(SecondRoomId, OnePlayer),
                CreateRoom(FirstRoomId, OnePlayer)
            };

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
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, OnePlayer) };
            var newRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, TwoPlayers) };
            oldRooms[0].AccessCode = TypedCode;

            RoomListUpdater.KeepTypedCodes(oldRooms, newRooms);

            Assert.Equal(TypedCode, newRooms[0].AccessCode);
        }

        [Fact]
        public void KeepTypedCodes_NewRoomInTheList_LeavesItsCodeEmpty()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, OnePlayer) };
            var newRooms = new List<RoomListItem> { CreateRoom(SecondRoomId, OnePlayer) };
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
