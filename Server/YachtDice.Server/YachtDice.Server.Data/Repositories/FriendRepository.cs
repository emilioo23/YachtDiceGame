using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
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
            return Query(context => context.Friendship
                .Where(friendship =>
                    (friendship.RequesterId == firstPlayerId && friendship.AddresseeId == secondPlayerId)
                    || (friendship.RequesterId == secondPlayerId && friendship.AddresseeId == firstPlayerId))
                .ToList());
        }

        /// <summary>
        /// Finds the players who are friends of the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The friends, or an empty list if the player has none.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<Player> FindFriends(int playerId)
        {
            return Query(context => SelectFriends(context, playerId));
        }

        /// <summary>
        /// Finds the players who sent a friend request to the player that has not been answered.
        /// </summary>
        /// <param name="playerId">Identifier of the player who received the requests.</param>
        /// <returns>The players who sent a request, or an empty list if there is none.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<Player> FindPendingRequesters(int playerId)
        {
            return Query(context => SelectPendingRequesters(context, playerId));
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

            Save(context =>
            {
                var friendship = new Friendship
                {
                    RequesterId = requesterId,
                    AddresseeId = addresseeId,
                    Status = status
                };

                context.Friendship.Add(friendship);
            });
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

            Save(context =>
            {
                Friendship friendship = context.Friendship.Find(requesterId, addresseeId);

                if (friendship != null)
                {
                    friendship.Status = status;
                }
            });
        }

        /// <summary>
        /// Deletes a friendship.
        /// </summary>
        /// <param name="requesterId">Identifier of the player who sent the request.</param>
        /// <param name="addresseeId">Identifier of the player who received the request.</param>
        /// <exception cref="DataAccessException">The friendship could not be deleted.</exception>
        public void Remove(int requesterId, int addresseeId)
        {
            Save(context =>
            {
                Friendship friendship = context.Friendship.Find(requesterId, addresseeId);

                if (friendship != null)
                {
                    context.Friendship.Remove(friendship);
                }
            });
        }

        private static List<Player> SelectFriends(YachtDiceContext context, int playerId)
        {
            var requested = context.Friendship
                .Where(friendship => friendship.RequesterId == playerId
                    && friendship.Status == FriendshipStatus.Accepted)
                .Select(friendship => friendship.AddresseeId);
            var received = context.Friendship
                .Where(friendship => friendship.AddresseeId == playerId
                    && friendship.Status == FriendshipStatus.Accepted)
                .Select(friendship => friendship.RequesterId);

            return context.Player
                .Where(player => requested.Contains(player.Id) || received.Contains(player.Id))
                .ToList();
        }

        private static List<Player> SelectPendingRequesters(YachtDiceContext context, int playerId)
        {
            var requesterIds = context.Friendship
                .Where(friendship => friendship.AddresseeId == playerId
                    && friendship.Status == FriendshipStatus.Pending)
                .Select(friendship => friendship.RequesterId);

            return context.Player.Where(player => requesterIds.Contains(player.Id)).ToList();
        }

        private static TResult Query<TResult>(Func<YachtDiceContext, TResult> query)
        {
            TResult result;

            try
            {
                using (var context = new YachtDiceContext())
                {
                    result = query(context);
                }
            }
            catch (EntityException ex)
            {
                throw new DataAccessException(QueryFailedMessage, ex);
            }

            return result;
        }

        private static void Save(Action<YachtDiceContext> change)
        {
            try
            {
                using (var context = new YachtDiceContext())
                {
                    change(context);
                    context.SaveChanges();
                }
            }
            catch (DbUpdateException ex)
            {
                throw new DataAccessException(UpdateFailedMessage, ex);
            }
            catch (EntityException ex)
            {
                throw new DataAccessException(UpdateFailedMessage, ex);
            }
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
