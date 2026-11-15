using System;
using System.Collections.Generic;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Data access operations for the friendships between players.
    /// </summary>
    public interface IFriendRepository
    {
        /// <summary>
        /// Finds the friendships between two players, no matter who sent the request.
        /// </summary>
        /// <param name="firstPlayerId">Identifier of the first player.</param>
        /// <param name="secondPlayerId">Identifier of the second player.</param>
        /// <returns>The friendships found, or an empty list if there is none.</returns>
        List<Friendship> FindBetween(int firstPlayerId, int secondPlayerId);

        /// <summary>
        /// Finds the players who are friends of the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The friends, or an empty list if the player has none.</returns>
        List<Player> FindFriends(int playerId);

        /// <summary>
        /// Finds the players who sent a friend request to the player that has not been answered.
        /// </summary>
        /// <param name="playerId">Identifier of the player who received the requests.</param>
        /// <returns>The players who sent a request, or an empty list if there is none.</returns>
        List<Player> FindPendingRequesters(int playerId);

        /// <summary>
        /// Counts the friends of a player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The number of accepted friendships of the player.</returns>
        int CountFriends(int playerId);

        /// <summary>
        /// Counts the friend requests that a player sent and that are still waiting for an answer.
        /// </summary>
        /// <param name="playerId">Identifier of the player who sent the requests.</param>
        /// <returns>The number of pending requests sent by the player.</returns>
        int CountPendingSent(int playerId);

        /// <summary>
        /// Saves a new friendship.
        /// </summary>
        /// <param name="friendship">Friendship to save, with the two players and its status.</param>
        void Add(Friendship friendship);

        /// <summary>
        /// Deletes the friend requests that are still waiting for an answer and were sent before a moment.
        /// </summary>
        /// <param name="sentBefore">Requests sent before this moment are deleted.</param>
        void RemoveExpiredRequests(DateTime sentBefore);

        /// <summary>
        /// Changes the status of a friendship.
        /// </summary>
        /// <param name="friendship">Friendship that already exists, with its new status.</param>
        void UpdateStatus(Friendship friendship);

        /// <summary>
        /// Deletes a friendship.
        /// </summary>
        /// <param name="friendship">Friendship to delete, identified by its two players.</param>
        void Remove(Friendship friendship);
    }
}
