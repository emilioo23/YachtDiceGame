using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using YachtDice.Server.Data.Exceptions;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the game history data access.
    /// </summary>
    public class GameHistoryRepository : IGameHistoryRepository
    {
        private const string FindByPlayerFailedMessage =
            "Database error occurred while fetching game history by player ID.";

        private const string RankingFailedMessage = "Database error occurred while calculating player rankings.";

        /// <summary>
        /// Gets the result of every game played by a player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The results of the player, or an empty list if there are none.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<GameHistoryPlayer> FindByPlayerId(int playerId)
        {
            List<GameHistoryPlayer> history = DatabaseOperation.Query(
                context => SelectByPlayerId(context, playerId),
                FindByPlayerFailedMessage);

            return history;
        }

        /// <summary>
        /// Gets the players with the most accumulated points, from highest to lowest.
        /// </summary>
        /// <param name="maxEntries">Maximum number of players to return.</param>
        /// <returns>The ranking, or an empty list if there are no games played.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<PlayerRankingRecord> GetRanking(int maxEntries)
        {
            List<PlayerRankingRecord> ranking = DatabaseOperation.Query(
                context => QueryRanking(context, maxEntries),
                RankingFailedMessage);

            return ranking;
        }

        private static List<GameHistoryPlayer> SelectByPlayerId(YachtDiceContext context, int playerId)
        {
            Expression<Func<GameHistoryPlayer, bool>> isSamePlayerId = entry => entry.PlayerId == playerId;
            List<GameHistoryPlayer> history = context.GameHistoryPlayer
                .Where(isSamePlayerId)
                .ToList();

            return history;
        }

        private static List<PlayerRankingRecord> QueryRanking(YachtDiceContext context, int maxEntries)
        {
            IQueryable<PlayerRankingRecord> records =
                from entry in context.GameHistoryPlayer
                group entry by entry.PlayerId into playerGames
                join player in context.Player on playerGames.Key equals player.Id
                let totalPoints = (from game in playerGames select game.FinalScore).Sum()
                let wins = (from game in playerGames where game.IsWinner == true select game).Count()
                orderby totalPoints descending, player.DisplayName
                select new PlayerRankingRecord
                {
                    DisplayName = player.DisplayName,
                    AvatarId = player.AvatarId,
                    TotalPoints = totalPoints,
                    Wins = wins
                };

            List<PlayerRankingRecord> ranking = records.Take(maxEntries).ToList();

            return ranking;
        }
    }
}