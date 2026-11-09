using System;
using Xunit;
using YachtDice.Server.Logic.Friends;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Friends
{
    /// <summary>
    /// Rules to invite a friend to a room: the player must be inside the room and the invited player must be a
    /// friend.
    /// </summary>
    public class RoomInvitationServiceTests
    {
        private const string HostName = "Emilio";
        private const string FriendName = "Gabriel";
        private const string StrangerName = "Carlos";
        private const string UnknownRoomCode = "ZZZZZZ";
        private const string UnknownPlayerId = "not-a-player";

        private readonly RoomManager _roomManager = new RoomManager(new InMemoryAvatarLookup());
        private readonly InMemoryFriendshipService _friendships = new InMemoryFriendshipService();
        private readonly RoomInvitationService _service;

        /// <summary>
        /// Prepares two players who are friends and a service that checks the invitations between them.
        /// </summary>
        public RoomInvitationServiceTests()
        {
            _friendships.AddFriends(HostName, FriendName);
            _service = new RoomInvitationService(_roomManager, _friendships);
        }

        [Fact]
        public void Constructor_NullRoomManager_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RoomInvitationService(null, _friendships));
        }

        [Fact]
        public void Constructor_NullFriendshipService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RoomInvitationService(_roomManager, null));
        }

        [Fact]
        public void Validate_PlayerInsideTheRoomAndInvitedPlayerIsAFriend_AllowsTheInvitation()
        {
            RoomResult host = _roomManager.CreateRoom(HostName);

            InvitationResult result = _service.Validate(host.PlayerId, FriendName, host.Room.Code);

            Assert.Equal(InvitationStatus.Allowed, result.Status);
            Assert.Equal(HostName, result.SenderName);
        }

        [Fact]
        public void Validate_FriendNameWithOtherCase_AllowsTheInvitation()
        {
            RoomResult host = _roomManager.CreateRoom(HostName);

            InvitationResult result = _service.Validate(host.PlayerId, FriendName.ToUpperInvariant(), host.Room.Code);

            Assert.Equal(InvitationStatus.Allowed, result.Status);
        }

        [Fact]
        public void Validate_AnyPlayerInsideTheRoom_AllowsTheInvitation()
        {
            RoomResult host = _roomManager.CreateRoom(HostName);
            RoomResult guest = _roomManager.Join(host.Room.Code, StrangerName);
            _friendships.AddFriends(StrangerName, FriendName);

            InvitationResult result = _service.Validate(guest.PlayerId, FriendName, host.Room.Code);

            Assert.Equal(InvitationStatus.Allowed, result.Status);
            Assert.Equal(StrangerName, result.SenderName);
        }

        [Fact]
        public void Validate_InvitedPlayerIsNotAFriend_ReturnsNotFriends()
        {
            RoomResult host = _roomManager.CreateRoom(HostName);

            InvitationResult result = _service.Validate(host.PlayerId, StrangerName, host.Room.Code);

            Assert.Equal(InvitationStatus.NotFriends, result.Status);
        }

        [Fact]
        public void Validate_PlayerIsNotInsideTheRoom_ReturnsNotInRoom()
        {
            RoomResult host = _roomManager.CreateRoom(HostName);

            InvitationResult result = _service.Validate(UnknownPlayerId, FriendName, host.Room.Code);

            Assert.Equal(InvitationStatus.NotInRoom, result.Status);
            Assert.Empty(result.SenderName);
        }

        [Fact]
        public void Validate_UnknownRoom_ReturnsNotInRoom()
        {
            RoomResult host = _roomManager.CreateRoom(HostName);

            InvitationResult result = _service.Validate(host.PlayerId, FriendName, UnknownRoomCode);

            Assert.Equal(InvitationStatus.NotInRoom, result.Status);
        }
    }
}
