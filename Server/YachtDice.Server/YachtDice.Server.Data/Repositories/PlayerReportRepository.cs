using System;
using System.Linq;
using System.Linq.Expressions;
using YachtDice.Server.Data.Exceptions;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the data access of the reports between players.
    /// </summary>
    public class PlayerReportRepository : IPlayerReportRepository
    {
        /// <summary>
        /// Saves a new report.
        /// </summary>
        /// <param name="report">Report with the two players, the reason and the game where it happened.</param>
        /// <exception cref="ArgumentNullException">The report is null.</exception>
        /// <exception cref="ArgumentException">The reason is empty.</exception>
        /// <exception cref="DataAccessException">The report could not be saved.</exception>
        public void Add(PlayerReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report), LogMessages.PlayerReportNullReportMessage);
            }

            TextGuard.RejectBlank(report.Reason, nameof(report.Reason));

            DatabaseOperation.Save(context => context.PlayerReport.Add(report),
                LogMessages.PlayerReportUpdateFailedMessage);
        }

        /// <summary>
        /// Indicates whether a player already reported another player after a moment.
        /// </summary>
        /// <param name="reporterId">Identifier of the player who reports.</param>
        /// <param name="reportedId">Identifier of the player who is reported.</param>
        /// <param name="since">Only the reports made after this moment count.</param>
        /// <returns>True if there is already a report between them in that time.</returns>
        /// <exception cref="DataAccessException">The reports could not be read.</exception>
        public bool HasReportSince(int reporterId, int reportedId, DateTime since)
        {
            Expression<Func<PlayerReport, bool>> isRecentReportOfPair = report =>
                report.ReporterId == reporterId && report.ReportedId == reportedId && report.ReportDate >= since;
            bool hasReport = DatabaseOperation.Query(
                context => ExistsReport(context, isRecentReportOfPair),
                LogMessages.PlayerReportQueryFailedMessage);

            return hasReport;
        }

        /// <summary>
        /// Counts how many different players reported a player after a moment.
        /// </summary>
        /// <param name="reportedId">Identifier of the player who is reported.</param>
        /// <param name="since">Only the reports made after this moment count.</param>
        /// <returns>The number of different players who reported the player in that time.</returns>
        /// <exception cref="DataAccessException">The reports could not be read.</exception>
        public int CountReportersSince(int reportedId, DateTime since)
        {
            int reporterCount = DatabaseOperation.Query(
                context => CountDistinctReporters(context, reportedId, since),
                LogMessages.PlayerReportQueryFailedMessage);

            return reporterCount;
        }

        /// <summary>
        /// Finds the last saved game where both players took part, finished after a moment.
        /// </summary>
        /// <param name="firstPlayerId">Identifier of one of the players.</param>
        /// <param name="secondPlayerId">Identifier of the other player.</param>
        /// <param name="since">Only the games finished after this moment count.</param>
        /// <returns>The identifier of the game, or zero if they did not play together in that time.</returns>
        /// <exception cref="DataAccessException">The games could not be read.</exception>
        public int FindLastSharedGameId(int firstPlayerId, int secondPlayerId, DateTime since)
        {
            Expression<Func<GameHistory, bool>> isRecentSharedGame = game =>
                game.EndDate >= since
                && game.GameHistoryPlayer.Any(historyPlayer => historyPlayer.PlayerId == firstPlayerId)
                && game.GameHistoryPlayer.Any(historyPlayer => historyPlayer.PlayerId == secondPlayerId);
            int gameId = DatabaseOperation.Query(
                context => SelectLastGameId(context, isRecentSharedGame),
                LogMessages.PlayerReportQueryFailedMessage);

            return gameId;
        }

        private static bool ExistsReport(YachtDiceContext context, Expression<Func<PlayerReport, bool>> isWantedReport)
        {
            return context.PlayerReport.Any(isWantedReport);
        }

        private static int CountDistinctReporters(YachtDiceContext context, int reportedId, DateTime since)
        {
            Expression<Func<PlayerReport, bool>> isRecentAgainstPlayer = report =>
                report.ReportedId == reportedId && report.ReportDate >= since;

            return context.PlayerReport
                .Where(isRecentAgainstPlayer)
                .Select(report => report.ReporterId)
                .Distinct()
                .Count();
        }

        private static int SelectLastGameId(YachtDiceContext context, Expression<Func<GameHistory, bool>> isWantedGame)
        {
            int gameId = context.GameHistory
                .Where(isWantedGame)
                .OrderByDescending(game => game.EndDate)
                .Select(game => game.Id)
                .FirstOrDefault();

            return gameId;
        }
    }
}
