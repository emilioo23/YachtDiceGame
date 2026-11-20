using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Keeps the friendships in memory so the tests do not need the database.
    /// </summary>
    internal sealed class InMemoryFriendRepository : IFriendRepository
    {
        private readonly List<Friendship> _friendships = new List<Friendship>();
        private readonly InMemoryPlayerRepository _players;

        /// <summary>
        /// Initializes the repository without players, for the tests that only count the friendships.
        /// </summary>
        public InMemoryFriendRepository()
            : this(new InMemoryPlayerRepository())
        {
        }

        /// <summary>
        /// Initializes the repository with the players that the friendships point to.
        /// </summary>
        /// <param name="players">Players of the test.</param>
        public InMemoryFriendRepository(InMemoryPlayerRepository players)
        {
            _players = players;
        }

        /// <summary>
        /// Gets the friendships saved so far.
        /// </summary>
        public IReadOnlyList<Friendship> Friendships => _friendships;

        /// <inheritdoc />
        public List<Friendship> FindBetween(int firstPlayerId, int secondPlayerId)
        {
            List<Friendship> matches = _friendships
                .Where(friendship => IsBetween(friendship, firstPlayerId, secondPlayerId))
                .ToList();

            return matches;
        }

        /// <inheritdoc />
        public List<Player> FindFriends(int playerId)
        {
            return new List<Player>();
        }

        /// <inheritdoc />
        public List<Player> FindPendingRequesters(int playerId)
        {
            return new List<Player>();
        }

        /// <inheritdoc />
        public List<Player> FindBlockedBy(int playerId)
        {
            List<int> blockedIds = _friendships
                .Where(friendship => IsBlockedBy(friendship, playerId))
                .Select(friendship => friendship.AddresseeId)
                .ToList();

            return _players.Players.Where(player => blockedIds.Contains(player.Id)).ToList();
        }

        /// <inheritdoc />
        public int CountFriends(int playerId)
        {
            Func<Friendship, bool> isAcceptedWithPlayer = friendship =>
                IsWithPlayer(friendship, playerId) && friendship.Status == FriendshipStatus.Accepted;

            return _friendships.Count(isAcceptedWithPlayer);
        }

        /// <inheritdoc />
        public int CountPendingSent(int playerId)
        {
            Func<Friendship, bool> isPendingFromPlayer =
                friendship => friendship.RequesterId == playerId && friendship.Status == FriendshipStatus.Pending;

            return _friendships.Count(isPendingFromPlayer);
        }

        /// <inheritdoc />
        public void Add(Friendship friendship)
        {
            _friendships.Add(friendship);
        }

        /// <inheritdoc />
        public void RemoveExpiredRequests(DateTime sentBefore)
        {
            _friendships.RemoveAll(friendship => IsExpiredRequest(friendship, sentBefore));
        }

        /// <inheritdoc />
        public void UpdateStatus(Friendship friendship)
        {
            Friendship stored = _friendships.First(row => IsSamePlayers(row, friendship));

            stored.Status = friendship.Status;
        }

        /// <inheritdoc />
        public void Remove(Friendship friendship)
        {
            _friendships.RemoveAll(row => IsSamePlayers(row, friendship));
        }

        private static bool IsSamePlayers(Friendship first, Friendship second)
        {
            bool isSameRequester = first.RequesterId == second.RequesterId;
            bool isSameAddressee = first.AddresseeId == second.AddresseeId;

            return isSameRequester && isSameAddressee;
        }

        private static bool IsBetween(Friendship friendship, int firstPlayerId, int secondPlayerId)
        {
            bool isRequested = friendship.RequesterId == firstPlayerId && friendship.AddresseeId == secondPlayerId;
            bool isReceived = friendship.RequesterId == secondPlayerId && friendship.AddresseeId == firstPlayerId;

            return isRequested || isReceived;
        }

        private static bool IsBlockedBy(Friendship friendship, int playerId)
        {
            return friendship.RequesterId == playerId && friendship.Status == FriendshipStatus.Blocked;
        }

        private static bool IsWithPlayer(Friendship friendship, int playerId)
        {
            return friendship.RequesterId == playerId || friendship.AddresseeId == playerId;
        }

        private static bool IsExpiredRequest(Friendship friendship, DateTime sentBefore)
        {
            return friendship.Status == FriendshipStatus.Pending && friendship.CreateDate < sentBefore;
        }
    }
}
