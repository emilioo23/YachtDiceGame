using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task<List<GameHistoryPlayer>> FindByPlayerIdAsync(int playerId);

        /// <summary>
        /// Gets the players with the most accumulated points, from highest to lowest.
        /// </summary>
        /// <param name="maxEntries">Maximum number of players to return.</param>
        /// <returns>The ranking, or an empty list if there are no games played.</returns>
        Task<List<PlayerRankingRecord>> GetRankingAsync(int maxEntries);
    }
}