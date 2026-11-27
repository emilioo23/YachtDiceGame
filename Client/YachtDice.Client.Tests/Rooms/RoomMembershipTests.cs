using System;
using Xunit;
using YachtDice.Client.Logic.Rooms;

namespace YachtDice.Client.Tests.Rooms
{
    /// <summary>
    /// The place of the player in a room: the identifier that the server gave and the code of the room, which the
    /// chat and the game send with every call.
    /// </summary>
    public class RoomMembershipTests
    {
        private const string PlayerId = "player-1";
        private const string RoomCode = "ABC123";

        [Fact]
        public void Constructor_NullPlayerId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RoomMembership(null, RoomCode));
        }

        [Fact]
        public void Constructor_NullRoomCode_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RoomMembership(PlayerId, null));
        }

        [Fact]
        public void Constructor_ValidValues_KeepsThePlayerId()
        {
            var membership = new RoomMembership(PlayerId, RoomCode);

            Assert.Equal(PlayerId, membership.PlayerId);
        }

        [Fact]
        public void Constructor_ValidValues_KeepsTheRoomCode()
        {
            var membership = new RoomMembership(PlayerId, RoomCode);

            Assert.Equal(RoomCode, membership.RoomCode);
        }
    }
}
