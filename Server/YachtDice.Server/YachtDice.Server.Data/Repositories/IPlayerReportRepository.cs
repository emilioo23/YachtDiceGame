using System;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Data access operations for the reports that a player makes about another player.
    /// </summary>
    public interface IPlayerReportRepository
    {
        /// <summary>
        /// Saves a new report.
        /// </summary>
        /// <param name="report">Report with the two players, the reason and the game where it happened.</param>
        void Add(PlayerReport report);

        /// <summary>
        /// Indicates whether a player already reported another player after a moment.
        /// </summary>
        /// <param name="reporterId">Identifier of the player who reports.</param>
        /// <param name="reportedId">Identifier of the player who is reported.</param>
        /// <param name="since">Only the reports made after this moment count.</param>
        /// <returns>True if there is already a report between them in that time.</returns>
        bool HasReportSince(int reporterId, int reportedId, DateTime since);

        /// <summary>
        /// Counts how many different players reported a player after a moment.
        /// </summary>
        /// <param name="reportedId">Identifier of the player who is reported.</param>
        /// <param name="since">Only the reports made after this moment count.</param>
        /// <returns>The number of different players who reported the player in that time.</returns>
        int CountReportersSince(int reportedId, DateTime since);

        /// <summary>
        /// Finds the last saved game where both players took part, finished after a moment.
        /// </summary>
        /// <param name="firstPlayerId">Identifier of one of the players.</param>
        /// <param name="secondPlayerId">Identifier of the other player.</param>
        /// <param name="since">Only the games finished after this moment count.</param>
        /// <returns>The identifier of the game, or zero if they did not play together in that time.</returns>
        int FindLastSharedGameId(int firstPlayerId, int secondPlayerId, DateTime since);
    }
}
