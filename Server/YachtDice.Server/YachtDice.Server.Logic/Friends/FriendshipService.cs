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
        private readonly IPlayerRepository _playerRepository;
        private readonly IFriendRepository _friendRepository;

        /// <summary>
        /// Initializes the service with the repositories it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to find the players.</param>
        /// <param name="friendRepository">Repository used to read and save the friendships.</param>
        public FriendshipService(IPlayerRepository playerRepository, IFriendRepository friendRepository)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository));
            _friendRepository = friendRepository
                ?? throw new ArgumentNullException(nameof(friendRepository));
        }

        /// <summary>
        /// Sends a friend request to the player who owns the username.
        /// </summary>
        /// <param name="displayName">Name of the player who sends the request.</param>
        /// <param name="targetUsername">Username of the player who will receive the request.</param>
        /// <returns>Outcome of the request and the name of the player who owns the username.</returns>
        public FriendRequestResult SendRequest(string displayName, string targetUsername)
        {
            var result = new FriendRequestResult(FriendRequestStatus.InvalidUsername, string.Empty);

            if (!string.IsNullOrWhiteSpace(targetUsername))
            {
                Player requester = _playerRepository.FindByName(displayName).FirstOrDefault();
                Player target = _playerRepository.FindByUsername(targetUsername.Trim()).FirstOrDefault();
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
            FriendActionStatus status = FriendActionStatus.PlayerNotFound;

            if (addressee != null && requester != null)
            {
                Func<Friendship, bool> isPendingFromRequester =
                    friendship => friendship.RequesterId == requester.Id
                    && friendship.Status == FriendshipStatus.Pending;
                Friendship friendRequest = _friendRepository.FindBetween(requester.Id, addressee.Id)
                    .FirstOrDefault(isPendingFromRequester);

                status = AnswerRequest(friendRequest, isAccepted);
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
            FriendActionStatus status = FriendActionStatus.PlayerNotFound;

            if (player != null && friend != null)
            {
                Func<Friendship, bool> isAccepted = row => row.Status == FriendshipStatus.Accepted;
                Friendship friendship = _friendRepository.FindBetween(player.Id, friend.Id)
                    .FirstOrDefault(isAccepted);

                status = DeleteFriendship(friendship);
            }

            return status;
        }

        /// <summary>
        /// Gets the username that other players use to send a friend request to the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The username of the player, or an empty text if the player does not exist.</returns>
        public string GetUsername(string displayName)
        {
            Player player = _playerRepository.FindByName(displayName).FirstOrDefault();

            return player?.Username ?? string.Empty;
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
            Func<string, bool> hasName = name => !string.IsNullOrWhiteSpace(name);
            List<string> names = players
                .Select(player => player.DisplayName)
                .Where(hasName)
                .OrderBy(name => name)
                .ToList();

            return names;
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
                status = FriendRequestStatus.UsernameNotFound;
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
                var request = new Friendship
                {
                    RequesterId = requester.Id,
                    AddresseeId = target.Id,
                    Status = FriendshipStatus.Pending
                };

                _friendRepository.Add(request);
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
                status = FriendRequestStatus.RequestAlreadyReceived;
            }

            return status;
        }

        private FriendActionStatus AnswerRequest(Friendship friendRequest, bool isAccepted)
        {
            FriendActionStatus status = FriendActionStatus.RelationNotFound;

            if (friendRequest != null && isAccepted)
            {
                friendRequest.Status = FriendshipStatus.Accepted;
                _friendRepository.UpdateStatus(friendRequest);
                status = FriendActionStatus.Success;
            }
            else if (friendRequest != null)
            {
                _friendRepository.Remove(friendRequest);
                status = FriendActionStatus.Success;
            }

            return status;
        }

        private FriendActionStatus DeleteFriendship(Friendship friendship)
        {
            FriendActionStatus status = FriendActionStatus.RelationNotFound;

            if (friendship != null)
            {
                _friendRepository.Remove(friendship);
                status = FriendActionStatus.Success;
            }

            return status;
        }
    }
}
