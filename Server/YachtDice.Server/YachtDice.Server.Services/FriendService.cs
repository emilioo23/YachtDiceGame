using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Friends;
    using LogicFriends = YachtDice.Server.Logic.Friends;
    using PresenceContracts = YachtDice.Server.Contracts.Presence;

    /// <summary>
    /// Manages the friends of the players: requests, answers, removals and lists.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
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
        /// Sends a friend request to the player who owns the username.
        /// </summary>
        /// <param name="displayName">Name of the player who sends the request.</param>
        /// <param name="targetUsername">Username of the player who will receive the request.</param>
        /// <returns>Outcome of the request.</returns>
        public Contracts.FriendRequestResultCode SendFriendRequest(string displayName, string targetUsername)
        {
            Contracts.FriendRequestResultCode result =
                Execute(displayName, () => ProcessRequest(displayName, targetUsername));

            return result;
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
            Contracts.FriendActionResultCode result =
                Execute(displayName, () => ProcessResponse(displayName, requesterName, isAccepted));

            return result;
        }

        /// <summary>
        /// Removes a friend from the list of the player.
        /// </summary>
        /// <param name="displayName">Name of the player who removes the friend.</param>
        /// <param name="friendName">Name of the friend to remove.</param>
        /// <returns>Outcome of the removal.</returns>
        public Contracts.FriendActionResultCode RemoveFriend(string displayName, string friendName)
        {
            Contracts.FriendActionResultCode result =
                Execute(displayName, () => ProcessRemoval(displayName, friendName));

            return result;
        }

        /// <summary>
        /// Gets the username that other players use to send a friend request to the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The username of the player, or an empty text if the player does not exist.</returns>
        public string GetUsername(string displayName)
        {
            string username = Execute(displayName, () => _friendshipService.GetUsername(displayName));

            return username;
        }

        /// <summary>
        /// Gets the names of the friends of the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the friends, or an empty list if the player has none.</returns>
        public List<string> GetFriends(string displayName)
        {
            List<string> friendNames = Execute(displayName, () => _friendshipService.GetFriendNames(displayName));

            return friendNames;
        }

        /// <summary>
        /// Gets the names of the players who sent a friend request that the player has not answered.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the players who sent a request, or an empty list if there is none.</returns>
        public List<string> GetFriendRequests(string displayName)
        {
            List<string> requesterNames = Execute(displayName, () => _friendshipService.GetRequesterNames(displayName));

            return requesterNames;
        }

        private Contracts.FriendRequestResultCode ProcessRequest(string displayName, string targetUsername)
        {
            LogicFriends.FriendRequestResult result = _friendshipService.SendRequest(displayName, targetUsername);

            if (result.Status == LogicFriends.FriendRequestStatus.RequestSent)
            {
                _notifier.SendEvent(
                    result.FriendName,
                    displayName,
                    PresenceContracts.PresenceEventType.FriendRequestReceived);
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
                _notifier.SendEvent(
                    requesterName,
                    displayName,
                    PresenceContracts.PresenceEventType.FriendRequestAccepted);
            }

            return Mapping.EnumMapper.Map<LogicFriends.FriendActionStatus, Contracts.FriendActionResultCode>(status);
        }

        private Contracts.FriendActionResultCode ProcessRemoval(string displayName, string friendName)
        {
            LogicFriends.FriendActionStatus status = _friendshipService.RemoveFriend(displayName, friendName);

            if (status == LogicFriends.FriendActionStatus.Success)
            {
                _notifier.SendEvent(friendName, displayName, PresenceContracts.PresenceEventType.FriendRemoved);
            }

            return Mapping.EnumMapper.Map<LogicFriends.FriendActionStatus, Contracts.FriendActionResultCode>(status);
        }
    }
}
