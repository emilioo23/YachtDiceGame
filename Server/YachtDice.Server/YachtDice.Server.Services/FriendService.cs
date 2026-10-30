using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;
    using LogicFriends = YachtDice.Server.Logic.Friends;

    /// <summary>
    /// Manages the friends of the players: requests, answers, removals and lists.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class FriendService : ServiceBase, Contracts.IFriendService
    {
        private const string NullDependencyMessage = "The dependency cannot be null.";

        private readonly LogicFriends.IFriendshipService _friendshipService;
        private readonly IPlayerNotifier _notifier;

        /// <summary>
        /// Initializes a new instance of the FriendService class.
        /// </summary>
        /// <param name="friendshipService">The service that applies the rules of the friendships.</param>
        /// <param name="notifier">The component that warns the players who are online.</param>
        public FriendService(LogicFriends.IFriendshipService friendshipService, IPlayerNotifier notifier)
        {
            _friendshipService = friendshipService
                ?? throw new ArgumentNullException(nameof(friendshipService), NullDependencyMessage);
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier), NullDependencyMessage);
        }

        /// <summary>
        /// Sends a friend request to the player who owns the friend code.
        /// </summary>
        /// <param name="displayName">Name of the player who sends the request.</param>
        /// <param name="friendCode">Friend code of the player who will receive the request.</param>
        /// <returns>Outcome of the request.</returns>
        public Contracts.FriendRequestResultCode SendFriendRequest(string displayName, string friendCode)
        {
            return Execute(displayName, () => ProcessRequest(displayName, friendCode));
        }

        /// <summary>
        /// Accepts or rejects a friend request that the player received.
        /// </summary>
        /// <param name="displayName">Name of the player who answers the request.</param>
        /// <param name="requesterName">Name of the player who sent the request.</param>
        /// <param name="isAccepted">True to accept the request, false to reject it.</param>
        /// <returns>Outcome of the answer.</returns>
        public Contracts.FriendActionResultCode RespondToFriendRequest(
            string displayName,
            string requesterName,
            bool isAccepted)
        {
            return Execute(displayName, () => ProcessResponse(displayName, requesterName, isAccepted));
        }

        /// <summary>
        /// Removes a friend from the list of the player.
        /// </summary>
        /// <param name="displayName">Name of the player who removes the friend.</param>
        /// <param name="friendName">Name of the friend to remove.</param>
        /// <returns>Outcome of the removal.</returns>
        public Contracts.FriendActionResultCode RemoveFriend(string displayName, string friendName)
        {
            return Execute(displayName, () => ProcessRemoval(displayName, friendName));
        }

        /// <summary>
        /// Gets the friend code that other players use to send a friend request to the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The friend code of the player, or an empty text if the player does not exist.</returns>
        public string GetFriendCode(string displayName)
        {
            return Execute(displayName, () => _friendshipService.GetFriendCode(displayName));
        }

        /// <summary>
        /// Gets the names of the friends of the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the friends, or an empty list if the player has none.</returns>
        public List<string> GetFriends(string displayName)
        {
            return Execute(displayName, () => _friendshipService.GetFriendNames(displayName));
        }

        /// <summary>
        /// Gets the names of the players who sent a friend request that the player has not answered.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the players who sent a request, or an empty list if there is none.</returns>
        public List<string> GetFriendRequests(string displayName)
        {
            return Execute(displayName, () => _friendshipService.GetRequesterNames(displayName));
        }

        private Contracts.FriendRequestResultCode ProcessRequest(string displayName, string friendCode)
        {
            LogicFriends.FriendRequestResult result = _friendshipService.SendRequest(displayName, friendCode);

            if (result.Status == LogicFriends.FriendRequestStatus.RequestSent)
            {
                _notifier.SendEvent(result.FriendName, displayName, Contracts.PresenceEventType.FriendRequestReceived);
            }
            else if (result.Status == LogicFriends.FriendRequestStatus.FriendAdded)
            {
                _notifier.SendEvent(result.FriendName, displayName, Contracts.PresenceEventType.FriendRequestAccepted);
            }

            return Mapping.EnumMapper.Map<LogicFriends.FriendRequestStatus, Contracts.FriendRequestResultCode>(
                result.Status);
        }

        private Contracts.FriendActionResultCode ProcessResponse(
            string displayName,
            string requesterName,
            bool isAccepted)
        {
            LogicFriends.FriendActionStatus status =
                _friendshipService.RespondToRequest(displayName, requesterName, isAccepted);

            if (status == LogicFriends.FriendActionStatus.Success && isAccepted)
            {
                _notifier.SendEvent(requesterName, displayName, Contracts.PresenceEventType.FriendRequestAccepted);
            }

            return Mapping.EnumMapper.Map<LogicFriends.FriendActionStatus, Contracts.FriendActionResultCode>(status);
        }

        private Contracts.FriendActionResultCode ProcessRemoval(string displayName, string friendName)
        {
            LogicFriends.FriendActionStatus status = _friendshipService.RemoveFriend(displayName, friendName);

            if (status == LogicFriends.FriendActionStatus.Success)
            {
                _notifier.SendEvent(friendName, displayName, Contracts.PresenceEventType.FriendRemoved);
            }

            return Mapping.EnumMapper.Map<LogicFriends.FriendActionStatus, Contracts.FriendActionResultCode>(status);
        }
    }
}
