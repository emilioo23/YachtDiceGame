using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the game history data access.
    /// </summary>
    public class GameHistoryRepository : IGameHistoryRepository
    {
        /// <summary>
        /// Gets the result of every game played by a player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The results of the player, or an empty list if there are none.</returns>
        public async Task<List<GameHistoryPlayer>> FindByPlayerIdAsync(int playerId)
        {
            using (var context = new YachtDiceContext())
            {
                return await context.GameHistoryPlayer
                    .Where(entry => entry.PlayerId == playerId)
                    .ToListAsync();
            }
        }
    }
}