using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Rules to add friends, answer friend requests and list the friends of a player.
    /// </summary>
    public class FriendshipService : IFriendshipService
    {
        private const string NullDependencyMessage = "The dependency cannot be null.";

        private readonly IPlayerRepository _playerRepository;
        private readonly IFriendRepository _friendRepository;

        /// <summary>
        /// Initializes the service with the repositories it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to find the players.</param>
        /// <param name="friendRepository">Repository used to read and save the friendships.</param>
        public FriendshipService(IPlayerRepository playerRepository, IFriendRepository friendRepository)
        {
            if (playerRepository == null)
            {
                throw new ArgumentNullException(nameof(playerRepository), NullDependencyMessage);
            }

            if (friendRepository == null)
            {
                throw new ArgumentNullException(nameof(friendRepository), NullDependencyMessage);
            }

            _playerRepository = playerRepository;
            _friendRepository = friendRepository;
        }

        /// <summary>
        /// Sends a friend request to the player who owns the friend code.
        /// </summary>
        /// <param name="displayName">Name of the player who sends the request.</param>
        /// <param name="friendCode">Friend code of the player who will receive the request.</param>
        /// <returns>Outcome of the request and the name of the player who owns the code.</returns>
        public FriendRequestResult SendRequest(string displayName, string friendCode)
        {
            var result = new FriendRequestResult(FriendRequestStatus.InvalidCode, string.Empty);

            if (!string.IsNullOrWhiteSpace(friendCode))
            {
                Player requester = _playerRepository.FindByName(displayName).FirstOrDefault();
                Player target = _playerRepository.FindByFriendCode(friendCode.Trim().ToUpperInvariant())
                    .FirstOrDefault();
                FriendRequestStatus status = ResolveRequest(requester, target);

                result = new FriendRequestResult(status, target?.DisplayName ?? string.Empty);
            }

            return result;
        }

        /// <summary>
        /// Accepts or rejects a friend request that the player received.
        /// </summary>
        /// <param name="displayName">Name of the player who answers the request.</param>
        /// <param name="requesterName">Name of the player who sent the request.</param>
        /// <param name="isAccepted">True to accept the request, false to reject it.</param>
        /// <returns>Outcome of the answer.</returns>
        public FriendActionStatus RespondToRequest(string displayName, string requesterName, bool isAccepted)
        {
            Player addressee = _playerRepository.FindByName(displayName).FirstOrDefault();
            Player requester = _playerRepository.FindByName(requesterName).FirstOrDefault();
            var status = FriendActionStatus.PlayerNotFound;

            if (addressee != null && requester != null)
            {
                Friendship request = _friendRepository.FindBetween(requester.Id, addressee.Id)
                    .FirstOrDefault(friendship => friendship.RequesterId == requester.Id
                        && friendship.Status == FriendshipStatus.Pending);

                status = AnswerRequest(request, isAccepted);
            }

            return status;
        }

        /// <summary>
        /// Removes a friend from the list of the player.
        /// </summary>
        /// <param name="displayName">Name of the player who removes the friend.</param>
        /// <param name="friendName">Name of the friend to remove.</param>
        /// <returns>Outcome of the removal.</returns>
        public FriendActionStatus RemoveFriend(string displayName, string friendName)
        {
            Player player = _playerRepository.FindByName(displayName).FirstOrDefault();
            Player friend = _playerRepository.FindByName(friendName).FirstOrDefault();
            var status = FriendActionStatus.PlayerNotFound;

            if (player != null && friend != null)
            {
                Friendship friendship = _friendRepository.FindBetween(player.Id, friend.Id)
                    .FirstOrDefault(row => row.Status == FriendshipStatus.Accepted);

                status = DeleteFriendship(friendship);
            }

            return status;
        }

        /// <summary>
        /// Gets the friend code that other players use to send a friend request to the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The friend code of the player, or an empty text if the player does not exist.</returns>
        public string GetFriendCode(string displayName)
        {
            Player player = _playerRepository.FindByName(displayName).FirstOrDefault();

            return player?.FriendCode ?? string.Empty;
        }

        /// <summary>
        /// Gets the names of the friends of the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the friends, or an empty list if the player has none.</returns>
        public List<string> GetFriendNames(string displayName)
        {
            var names = new List<string>();
            Player player = _playerRepository.FindByName(displayName).FirstOrDefault();

            if (player != null)
            {
                names = ToNames(_friendRepository.FindFriends(player.Id));
            }

            return names;
        }

        /// <summary>
        /// Gets the names of the players who sent a friend request that the player has not answered.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the players who sent a request, or an empty list if there is none.</returns>
        public List<string> GetRequesterNames(string displayName)
        {
            var names = new List<string>();
            Player player = _playerRepository.FindByName(displayName).FirstOrDefault();

            if (player != null)
            {
                names = ToNames(_friendRepository.FindPendingRequesters(player.Id));
            }

            return names;
        }

        private static List<string> ToNames(List<Player> players)
        {
            return players
                .Select(player => player.DisplayName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .OrderBy(name => name)
                .ToList();
        }

        private FriendRequestStatus ResolveRequest(Player requester, Player target)
        {
            FriendRequestStatus status;

            if (requester == null)
            {
                status = FriendRequestStatus.PlayerNotFound;
            }
            else if (target == null)
            {
                status = FriendRequestStatus.CodeNotFound;
            }
            else if (target.Id == requester.Id)
            {
                status = FriendRequestStatus.SelfRequest;
            }
            else
            {
                status = CreateRequest(requester, target);
            }

            return status;
        }

        private FriendRequestStatus CreateRequest(Player requester, Player target)
        {
            Friendship existing = _friendRepository.FindBetween(requester.Id, target.Id).FirstOrDefault();
            FriendRequestStatus status;

            if (existing == null)
            {
                _friendRepository.Add(requester.Id, target.Id, FriendshipStatus.Pending);
                status = FriendRequestStatus.RequestSent;
            }
            else if (existing.Status == FriendshipStatus.Accepted)
            {
                status = FriendRequestStatus.AlreadyFriends;
            }
            else if (existing.RequesterId == requester.Id)
            {
                status = FriendRequestStatus.AlreadyRequested;
            }
            else
            {
                _friendRepository.UpdateStatus(existing.RequesterId, existing.AddresseeId, FriendshipStatus.Accepted);
                status = FriendRequestStatus.FriendAdded;
            }

            return status;
        }

        private FriendActionStatus AnswerRequest(Friendship request, bool isAccepted)
        {
            var status = FriendActionStatus.RelationNotFound;

            if (request != null && isAccepted)
            {
                _friendRepository.UpdateStatus(request.RequesterId, request.AddresseeId, FriendshipStatus.Accepted);
                status = FriendActionStatus.Success;
            }
            else if (request != null)
            {
                _friendRepository.Remove(request.RequesterId, request.AddresseeId);
                status = FriendActionStatus.Success;
            }

            return status;
        }

        private FriendActionStatus DeleteFriendship(Friendship friendship)
        {
            var status = FriendActionStatus.RelationNotFound;

            if (friendship != null)
            {
                _friendRepository.Remove(friendship.RequesterId, friendship.AddresseeId);
                status = FriendActionStatus.Success;
            }

            return status;
        }
    }
}
