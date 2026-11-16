using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Rooms;
using RoomInvite = YachtDice.Server.Data.RoomInvite;

namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Rules to invite a friend to a room: only a player who is inside the room can invite, and only a friend.
    /// The invitations that reach a registered friend are saved, so the friend can see them later in a list.
    /// </summary>
    public class RoomInvitationService : IRoomInvitationService
    {
        private const string NullDependencyMessage = "A dependency of the room invitation service cannot be null.";
        private const int InvitationLifetimeHours = 24;
        private const int MaxReceivedInvitations = 20;

        private readonly IRoomManager _roomManager;
        private readonly IFriendshipService _friendshipService;
        private readonly IRoomInviteRepository _inviteRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomInvitationService"/> class.
        /// </summary>
        /// <param name="roomManager">The rooms, to know who is inside each one.</param>
        /// <param name="friendshipService">The friendships, to know who is a friend of whom.</param>
        /// <param name="inviteRepository">The repository where the invitations are saved.</param>
        public RoomInvitationService(
            IRoomManager roomManager,
            IFriendshipService friendshipService,
            IRoomInviteRepository inviteRepository)
        {
            _roomManager = roomManager
                ?? throw new ArgumentNullException(nameof(roomManager), NullDependencyMessage);
            _friendshipService = friendshipService
                ?? throw new ArgumentNullException(nameof(friendshipService), NullDependencyMessage);
            _inviteRepository = inviteRepository
                ?? throw new ArgumentNullException(nameof(inviteRepository), NullDependencyMessage);
        }

        /// <inheritdoc />
        public InvitationResult CheckInvitation(RoomMember sender, string friendName)
        {
            InvitationResult result;

            if (_roomManager.TryGetPlayer(sender, out RoomPlayer senderPlayer))
            {
                result = CheckFriendship(senderPlayer.DisplayName, friendName);
            }
            else
            {
                result = new InvitationResult(InvitationStatus.NotInRoom, string.Empty);
            }

            return result;
        }

        /// <inheritdoc />
        public void SaveInvitation(string senderName, string friendName, string roomCode)
        {
            int senderId = _friendshipService.FindPlayerId(senderName);
            int friendId = _friendshipService.FindPlayerId(friendName);
            bool areRegistered = senderId != FriendshipService.NoPlayerId && friendId != FriendshipService.NoPlayerId;

            if (areRegistered)
            {
                _inviteRepository.Add(new RoomInvite
                {
                    InviterId = senderId,
                    InvitedId = friendId,
                    RoomCode = roomCode,
                    SendDate = DateTime.UtcNow
                });
            }
        }

        /// <inheritdoc />
        public List<ReceivedInvitation> GetReceivedInvitations(string displayName)
        {
            int playerId = _friendshipService.FindPlayerId(displayName);
            var invitations = new List<ReceivedInvitation>();

            if (playerId != FriendshipService.NoPlayerId)
            {
                DateTime since = DateTime.UtcNow.AddHours(-InvitationLifetimeHours);
                invitations = _inviteRepository.FindReceived(playerId, since, MaxReceivedInvitations)
                    .Select(ToReceivedInvitation)
                    .ToList();
            }

            return invitations;
        }

        private ReceivedInvitation ToReceivedInvitation(RoomInviteRecord record)
        {
            return new ReceivedInvitation
            {
                SenderName = record.InviterName,
                RoomCode = record.RoomCode,
                SentAt = record.SendDate,
                IsRoomOpen = _roomManager.TryGetHost(record.RoomCode, out _)
            };
        }

        private InvitationResult CheckFriendship(string senderName, string friendName)
        {
            InvitationStatus status = InvitationStatus.NotFriends;

            if (_friendshipService.GetFriendNames(senderName).Contains(friendName, StringComparer.OrdinalIgnoreCase))
            {
                status = InvitationStatus.Allowed;
            }

            return new InvitationResult(status, senderName);
        }
    }
}
