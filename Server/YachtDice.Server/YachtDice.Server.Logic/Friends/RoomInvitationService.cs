using System;
using System.Linq;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Rules to invite a friend to a room: only a player who is inside the room can invite, and only a friend.
    /// </summary>
    public class RoomInvitationService : IRoomInvitationService
    {
        private readonly IRoomManager _roomManager;
        private readonly IFriendshipService _friendshipService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomInvitationService"/> class.
        /// </summary>
        /// <param name="roomManager">The rooms, to know who is inside each one.</param>
        /// <param name="friendshipService">The friendships, to know who is a friend of whom.</param>
        public RoomInvitationService(IRoomManager roomManager, IFriendshipService friendshipService)
        {
            _roomManager = roomManager
                ?? throw new ArgumentNullException(nameof(roomManager));
            _friendshipService = friendshipService
                ?? throw new ArgumentNullException(nameof(friendshipService));
        }

        /// <inheritdoc />
        public InvitationResult Validate(string playerId, string friendName, string roomCode)
        {
            InvitationResult result;

            if (_roomManager.TryGetPlayer(roomCode, playerId, out RoomPlayer sender))
            {
                result = CheckFriendship(sender.DisplayName, friendName);
            }
            else
            {
                result = new InvitationResult(InvitationStatus.NotInRoom, string.Empty);
            }

            return result;
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
