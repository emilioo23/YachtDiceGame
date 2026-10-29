using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Linq;

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
        public List<GameHistoryPlayer> FindByPlayerId(int playerId)
        {
            try
            {
                using (var context = new YachtDiceContext())
                {
                    return context.GameHistoryPlayer
                        .Where(entry => entry.PlayerId == playerId)
                        .ToList();
                }
            }
            catch (EntityException ex)
            {
                throw new DataException("Database error occurred while fetching game history by player ID.", ex);
            }
        }

        /// <summary>
        /// Gets the players with the most accumulated points, from highest to lowest.
        /// </summary>
        /// <param name="maxEntries">Maximum number of players to return.</param>
        /// <returns>The ranking, or an empty list if there are no games played.</returns>
        public List<PlayerRankingRecord> GetRanking(int maxEntries)
        {
            try
            {
                using (var context = new YachtDiceContext())
                {
                    return context.GameHistoryPlayer
                        .GroupBy(entry => entry.PlayerId)
                        .Select(group => new
                        {
                            PlayerId = group.Key,
                            TotalPoints = group.Sum(entry => entry.FinalScore),
                            Wins = group.Count(entry => entry.IsWinner == true)
                        })
                        .Join(
                            context.Player,
                            totals => totals.PlayerId,
                            player => player.Id,
                            (totals, player) => new PlayerRankingRecord
                            {
                                DisplayName = player.DisplayName,
                                TotalPoints = totals.TotalPoints,
                                Wins = totals.Wins
                            })
                        .OrderByDescending(record => record.TotalPoints)
                        .ThenBy(record => record.DisplayName)
                        .Take(maxEntries)
                        .ToList();
                }
            }
            catch (EntityException ex)
            {
                throw new DataException("Database error occurred while calculating player rankings.", ex);
            }
        }
    }
}