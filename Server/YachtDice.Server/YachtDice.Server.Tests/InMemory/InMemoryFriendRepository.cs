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
        public void Add(Friendship friendship)
        {
            _friendships.Add(friendship);
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
    }
}
