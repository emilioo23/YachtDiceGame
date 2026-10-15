using System.Collections.Generic;
using System.Threading.Tasks;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Data access for the results of the finished games.
    /// </summary>
    public interface IGameHistoryRepository
    {
        /// <summary>
        /// Gets the result of every game played by a player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The results of the player, or an empty list if there are none.</returns>
        Task<List<GameHistoryPlayer>> FindByPlayerIdAsync(int playerId);
    }
}