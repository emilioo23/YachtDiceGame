using System.Collections.Generic;
using YachtDice.Server.Data.Exceptions;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Data access for the results of the games already played.
    /// </summary>
    public interface IGameHistoryRepository
    {
        /// <summary>
        /// Gets the result of every game played by a player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The results of the player, or an empty list if there are none.</returns>
        List<GameHistoryPlayer> FindByPlayerId(int playerId);

        /// <summary>
        /// Gets the players with the most accumulated points, from highest to lowest.
        /// </summary>
        /// <param name="maxEntries">Maximum number of players to return.</param>
        /// <returns>The ranking, or an empty list if there are no games played.</returns>
        List<PlayerRankingRecord> GetRanking(int maxEntries);

        /// <summary>
        /// Saves the result of a game that just finished, with the players who took part in it.
        /// </summary>
        /// <param name="result">Result of the game.</param>
        /// <exception cref="DataAccessException">The database could not be updated.</exception>
        void Save(GameResultRecord result);
    }
}