using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data.Exceptions;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the friendship data access.
    /// </summary>
    public class FriendRepository : IFriendRepository
    {
        private const string EmptyValueMessage = "The value cannot be empty.";
        private const string QueryFailedMessage = "Database error occurred while querying the friendships.";
        private const string UpdateFailedMessage = "Database error occurred while saving the friendships.";

        /// <summary>
        /// Finds the friendships between two players, no matter who sent the request.
        /// </summary>
        /// <param name="firstPlayerId">Identifier of the first player.</param>
        /// <param name="secondPlayerId">Identifier of the second player.</param>
        /// <returns>The friendships found, or an empty list if there is none.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<Friendship> FindBetween(int firstPlayerId, int secondPlayerId)
        {
            List<Friendship> friendships = Query(context => SelectBetween(context, firstPlayerId, secondPlayerId));

            return friendships;
        }

        /// <summary>
        /// Finds the players who are friends of the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The friends, or an empty list if the player has none.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<Player> FindFriends(int playerId)
        {
            List<Player> friends = Query(context => SelectFriends(context, playerId));

            return friends;
        }

        /// <summary>
        /// Finds the players who sent a friend request to the player that has not been answered.
        /// </summary>
        /// <param name="playerId">Identifier of the player who received the requests.</param>
        /// <returns>The players who sent a request, or an empty list if there is none.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<Player> FindPendingRequesters(int playerId)
        {
            List<Player> requesters = Query(context => SelectPendingRequesters(context, playerId));

            return requesters;
        }

        /// <summary>
        /// Saves a new friendship.
        /// </summary>
        /// <param name="requesterId">Identifier of the player who sends the request.</param>
        /// <param name="addresseeId">Identifier of the player who receives the request.</param>
        /// <param name="status">Status of the friendship.</param>
        /// <exception cref="ArgumentException">The status is empty.</exception>
        /// <exception cref="DataAccessException">The friendship could not be saved.</exception>
        public void Add(int requesterId, int addresseeId, string status)
        {
            ValidateText(status, nameof(status));

            var friendship = new Friendship
            {
                RequesterId = requesterId,
                AddresseeId = addresseeId,
                Status = status
            };

            Save(context => context.Friendship.Add(friendship));
        }

        /// <summary>
        /// Changes the status of a friendship.
        /// </summary>
        /// <param name="requesterId">Identifier of the player who sent the request.</param>
        /// <param name="addresseeId">Identifier of the player who received the request.</param>
        /// <param name="status">New status of the friendship.</param>
        /// <exception cref="ArgumentException">The status is empty.</exception>
        /// <exception cref="DataAccessException">The friendship could not be saved.</exception>
        public void UpdateStatus(int requesterId, int addresseeId, string status)
        {
            ValidateText(status, nameof(status));

            var changes = new Friendship
            {
                RequesterId = requesterId,
                AddresseeId = addresseeId,
                Status = status
            };

            Save(context => ApplyStatus(context, changes));
        }

        /// <summary>
        /// Deletes a friendship.
        /// </summary>
        /// <param name="requesterId">Identifier of the player who sent the request.</param>
        /// <param name="addresseeId">Identifier of the player who received the request.</param>
        /// <exception cref="DataAccessException">The friendship could not be deleted.</exception>
        public void Remove(int requesterId, int addresseeId)
        {
            Save(context => DeleteFriendship(context, requesterId, addresseeId));
        }

        private static List<Friendship> SelectBetween(YachtDiceContext context, int firstPlayerId, int secondPlayerId)
        {
            List<Friendship> friendships = context.Friendship
                .Where(friendship =>
                    (friendship.RequesterId == firstPlayerId && friendship.AddresseeId == secondPlayerId)
                    || (friendship.RequesterId == secondPlayerId && friendship.AddresseeId == firstPlayerId))
                .ToList();

            return friendships;
        }

        private static void ApplyStatus(YachtDiceContext context, Friendship changes)
        {
            Friendship friendship = context.Friendship.Find(changes.RequesterId, changes.AddresseeId);

            if (friendship != null)
            {
                friendship.Status = changes.Status;
            }
        }

        private static void DeleteFriendship(YachtDiceContext context, int requesterId, int addresseeId)
        {
            Friendship friendship = context.Friendship.Find(requesterId, addresseeId);

            if (friendship != null)
            {
                context.Friendship.Remove(friendship);
            }
        }

        private static List<Player> SelectFriends(YachtDiceContext context, int playerId)
        {
            IQueryable<int> requested = context.Friendship
                .Where(friendship => friendship.RequesterId == playerId
                    && friendship.Status == FriendshipStatus.Accepted)
                .Select(friendship => friendship.AddresseeId);
            IQueryable<int> received = context.Friendship
                .Where(friendship => friendship.AddresseeId == playerId
                    && friendship.Status == FriendshipStatus.Accepted)
                .Select(friendship => friendship.RequesterId);

            List<Player> friends = context.Player
                .Where(player => requested.Contains(player.Id) || received.Contains(player.Id))
                .ToList();

            return friends;
        }

        private static List<Player> SelectPendingRequesters(YachtDiceContext context, int playerId)
        {
            IQueryable<int> requesterIds = context.Friendship
                .Where(friendship => friendship.AddresseeId == playerId
                    && friendship.Status == FriendshipStatus.Pending)
                .Select(friendship => friendship.RequesterId);

            List<Player> requesters = context.Player.Where(player => requesterIds.Contains(player.Id)).ToList();

            return requesters;
        }

        private static TResult Query<TResult>(Func<YachtDiceContext, TResult> query)
        {
            return DatabaseOperation.Query(query, QueryFailedMessage);
        }

        private static void Save(Action<YachtDiceContext> change)
        {
            DatabaseOperation.Save(change, UpdateFailedMessage);
        }

        private static void ValidateText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(EmptyValueMessage, parameterName);
            }
        }
    }
}
