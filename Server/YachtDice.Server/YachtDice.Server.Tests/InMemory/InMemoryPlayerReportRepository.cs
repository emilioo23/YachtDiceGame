using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Keeps the reports between players and the games they played together in memory, so the tests do not need
    /// the database.
    /// </summary>
    internal sealed class InMemoryPlayerReportRepository : IPlayerReportRepository
    {
        private const int NoGameId = 0;

        private readonly List<PlayerReport> _reports = new List<PlayerReport>();
        private readonly List<GameHistory> _games = new List<GameHistory>();

        /// <summary>
        /// Gets the reports saved so far.
        /// </summary>
        public IReadOnlyList<PlayerReport> Reports => _reports;

        /// <summary>
        /// Registers a finished game with the players who took part, so they can report each other.
        /// </summary>
        /// <param name="game">Game with its identifier, its end date and its players.</param>
        public void AddGame(GameHistory game)
        {
            _games.Add(game);
        }

        /// <inheritdoc />
        public void Add(PlayerReport report)
        {
            _reports.Add(report);
        }

        /// <inheritdoc />
        public bool HasReportSince(int reporterId, int reportedId, DateTime since)
        {
            Func<PlayerReport, bool> isRecentFromPair = report =>
                report.ReporterId == reporterId && report.ReportedId == reportedId && report.ReportDate >= since;

            return _reports.Any(isRecentFromPair);
        }

        /// <inheritdoc />
        public int CountReportersSince(int reportedId, DateTime since)
        {
            Func<PlayerReport, bool> isRecentAgainstPlayer = report =>
                report.ReportedId == reportedId && report.ReportDate >= since;

            return _reports.Where(isRecentAgainstPlayer).Select(report => report.ReporterId).Distinct().Count();
        }

        /// <inheritdoc />
        public int FindLastSharedGameId(int firstPlayerId, int secondPlayerId, DateTime since)
        {
            Func<GameHistory, bool> isRecentWithBoth = game =>
                game.EndDate >= since && HasPlayer(game, firstPlayerId) && HasPlayer(game, secondPlayerId);
            GameHistory lastGame = _games
                .Where(isRecentWithBoth)
                .OrderByDescending(game => game.EndDate)
                .FirstOrDefault();
            int gameId = NoGameId;

            if (lastGame != null)
            {
                gameId = lastGame.Id;
            }

            return gameId;
        }

        private static bool HasPlayer(GameHistory game, int playerId)
        {
            Func<GameHistoryPlayer, bool> isPlayer = historyPlayer => historyPlayer.PlayerId == playerId;

            return game.GameHistoryPlayer.Any(isPlayer);
        }
    }
}
