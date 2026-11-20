using System;
using System.Collections.Generic;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Friends;
using YachtDice.Server.Logic.Moderation;
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
        private const int HostId = 1;
        private const int FriendId = 2;

        private readonly RoomManager _roomManager = new RoomManager(
            new InMemoryAvatarLookup(),
            new OffensiveWordFilter(new string[0]));
        private readonly InMemoryFriendshipService _friendships = new InMemoryFriendshipService();
        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryRoomInviteRepository _invites;
        private readonly RoomInvitationService _service;

        /// <summary>
        /// Prepares two players who are friends and a service that checks the invitations between them.
        /// </summary>
        public RoomInvitationServiceTests()
        {
            _friendships.AddFriends(HostName, FriendName);
            _friendships.AddAccount(HostName, HostId);
            _friendships.AddAccount(FriendName, FriendId);
            _players.Add(new Player { Id = HostId, DisplayName = HostName });
            _invites = new InMemoryRoomInviteRepository(_players);
            _service = new RoomInvitationService(_roomManager, _friendships, _invites);
        }

        [Fact]
        public void GetReceivedInvitations_InvitationToAnOpenRoom_ShowsTheSenderAndThatTheRoomIsOpen()
        {
            RoomResult host = _roomManager.CreateRoom(HostName);
            _service.SaveInvitation(HostName, FriendName, host.Room.Code);
            var invitationsExpected = new[]
            {
                new { SenderName = HostName, RoomCode = host.Room.Code, IsRoomOpen = true }
            };

            List<ReceivedInvitation> invitations = _service.GetReceivedInvitations(FriendName);

            Assert.Equivalent(invitationsExpected, invitations);
        }

        [Fact]
        public void GetReceivedInvitations_RoomThatNoLongerExists_LeavesTheInvitationOut()
        {
            _service.SaveInvitation(HostName, FriendName, UnknownRoomCode);

            List<ReceivedInvitation> invitations = _service.GetReceivedInvitations(FriendName);

            Assert.Empty(invitations);
        }

        [Fact]
        public void SaveInvitation_GuestWithoutAccount_SavesNothing()
        {
            _service.SaveInvitation(StrangerName, FriendName, UnknownRoomCode);

            Assert.Empty(_invites.Invites);
        }

        [Fact]
        public void GetReceivedInvitations_PlayerWithoutAccount_ReturnsAnEmptyList()
        {
            List<ReceivedInvitation> invitations = _service.GetReceivedInvitations(StrangerName);

            Assert.Empty(invitations);
        }

        [Fact]
        public void Constructor_NullInviteRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RoomInvitationService(_roomManager, _friendships, null));
        }

        [Fact]
        public void Constructor_NullRoomManager_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RoomInvitationService(null, _friendships, _invites));
        }

        [Fact]
        public void Constructor_NullFriendshipService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RoomInvitationService(_roomManager, null, _invites));
        }

        [Fact]
        public void Validate_PlayerInsideTheRoomAndInvitedPlayerIsAFriend_AllowsTheInvitation()
        {
            RoomResult host = _roomManager.CreateRoom(HostName);

            InvitationResult invitationResultExpected = new InvitationResult(InvitationStatus.Allowed, HostName);

            InvitationResult invitationResult =
                _service.CheckInvitation(new RoomMember(host.Room.Code, host.PlayerId), FriendName);

            Assert.Equivalent(invitationResultExpected, invitationResult);
        }

        [Fact]
        public void Validate_FriendNameWithOtherCase_AllowsTheInvitation()
        {
            RoomResult host = _roomManager.CreateRoom(HostName);

            InvitationResult invitationResult =
                _service.CheckInvitation(new RoomMember(host.Room.Code, host.PlayerId), FriendName.ToUpperInvariant());

            Assert.Equal(InvitationStatus.Allowed, invitationResult.Status);
        }

        [Fact]
        public void Validate_AnyPlayerInsideTheRoom_AllowsTheInvitation()
        {
            RoomResult host = _roomManager.CreateRoom(HostName);
            RoomResult guest = _roomManager.Join(host.Room.Code, StrangerName);
            _friendships.AddFriends(StrangerName, FriendName);

            InvitationResult invitationResultExpected = new InvitationResult(InvitationStatus.Allowed, StrangerName);

            InvitationResult invitationResult =
                _service.CheckInvitation(new RoomMember(host.Room.Code, guest.PlayerId), FriendName);

            Assert.Equivalent(invitationResultExpected, invitationResult);
        }

        [Fact]
        public void Validate_InvitedPlayerIsNotAFriend_ReturnsNotFriends()
        {
            RoomResult host = _roomManager.CreateRoom(HostName);

            InvitationResult invitationResult =
                _service.CheckInvitation(new RoomMember(host.Room.Code, host.PlayerId), StrangerName);

            Assert.Equal(InvitationStatus.NotFriends, invitationResult.Status);
        }

        [Fact]
        public void Validate_PlayerIsNotInsideTheRoom_ReturnsNotInRoom()
        {
            RoomResult host = _roomManager.CreateRoom(HostName);

            InvitationResult invitationResultExpected = new InvitationResult(InvitationStatus.NotInRoom, string.Empty);

            InvitationResult invitationResult =
                _service.CheckInvitation(new RoomMember(host.Room.Code, UnknownPlayerId), FriendName);

            Assert.Equivalent(invitationResultExpected, invitationResult);
        }

        [Fact]
        public void Validate_UnknownRoom_ReturnsNotInRoom()
        {
            RoomResult host = _roomManager.CreateRoom(HostName);

            InvitationResult invitationResult =
                _service.CheckInvitation(new RoomMember(UnknownRoomCode, host.PlayerId), FriendName);

            Assert.Equal(InvitationStatus.NotInRoom, invitationResult.Status);
        }
    }
}
