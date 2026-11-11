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
        public void Add(int requesterId, int addresseeId, string status)
        {
            _friendships.Add(new Friendship { RequesterId = requesterId, AddresseeId = addresseeId, Status = status });
        }

        /// <inheritdoc />
        public void UpdateStatus(int requesterId, int addresseeId, string status)
        {
            Func<Friendship, bool> isSamePair = row => row.RequesterId == requesterId && row.AddresseeId == addresseeId;
            Friendship friendship = _friendships.First(isSamePair);

            friendship.Status = status;
        }

        /// <inheritdoc />
        public void Remove(int requesterId, int addresseeId)
        {
            Predicate<Friendship> isSamePair = row => row.RequesterId == requesterId && row.AddresseeId == addresseeId;
            _friendships.RemoveAll(isSamePair);
        }

        private static bool IsBetween(Friendship friendship, int firstPlayerId, int secondPlayerId)
        {
            bool isRequested = friendship.RequesterId == firstPlayerId && friendship.AddresseeId == secondPlayerId;
            bool isReceived = friendship.RequesterId == secondPlayerId && friendship.AddresseeId == firstPlayerId;

            return isRequested || isReceived;
        }
    }
}
