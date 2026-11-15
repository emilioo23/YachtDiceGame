using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Keeps the saved results of the games in memory so the tests do not need the database.
    /// </summary>
    internal sealed class InMemoryGameHistoryRepository : IGameHistoryRepository
    {
        private readonly List<GameResultRecord> _savedResults = new List<GameResultRecord>();
        private readonly List<GameHistoryPlayer> _entries = new List<GameHistoryPlayer>();
        private readonly List<GameHistoryPlayer> _computerGames = new List<GameHistoryPlayer>();
        private readonly List<PlayedGameRecord> _playedGames = new List<PlayedGameRecord>();
        private readonly List<PlayerRankingRecord> _rankingRecords = new List<PlayerRankingRecord>();

        /// <summary>
        /// Gets the results saved so far, in the order in which they were saved.
        /// </summary>
        public IReadOnlyList<GameResultRecord> SavedResults => _savedResults;

        /// <inheritdoc />
        public List<GameHistoryPlayer> FindByPlayerId(int playerId)
        {
            Func<GameHistoryPlayer, bool> belongsToPlayer = historyPlayer => historyPlayer.PlayerId == playerId;

            return _entries.Where(belongsToPlayer).ToList();
        }

        /// <summary>
        /// Adds a game that a player already played, so it counts in the statistics of the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="finalScore">Score of the player in the game.</param>
        /// <param name="isWinner">True if the player won the game.</param>
        public void AddPlayedGame(int playerId, int finalScore, bool isWinner)
        {
            _entries.Add(new GameHistoryPlayer { PlayerId = playerId, FinalScore = finalScore, IsWinner = isWinner });
        }

        /// <inheritdoc />
        public List<GameHistoryPlayer> FindComputerGames(int playerId)
        {
            Func<GameHistoryPlayer, bool> belongsToPlayer = historyPlayer => historyPlayer.PlayerId == playerId;

            return _computerGames.Where(belongsToPlayer).ToList();
        }

        public void AddComputerGame(int playerId, int finalScore, bool isWinner)
        {
            var historyPlayer = new GameHistoryPlayer
            {
                PlayerId = playerId,
                FinalScore = finalScore,
                IsWinner = isWinner
            };

            _entries.Add(historyPlayer);
            _computerGames.Add(historyPlayer);
        }

        public List<PlayedGameRecord> FindRecentGames(int playerId, int maxGames)
        {
            Func<PlayedGameRecord, bool> belongsToPlayer = record => record.PlayerId == playerId;
            Func<PlayedGameRecord, DateTime?> readEndDate = record => record.EndDate;
            Func<PlayedGameRecord, int> readGameId = record => record.GameId;
            List<int> recentIds = _playedGames
                .Where(belongsToPlayer)
                .OrderByDescending(readEndDate)
                .Select(readGameId)
                .Take(maxGames)
                .ToList();
            Func<PlayedGameRecord, bool> isInRecentGames = record => recentIds.Contains(record.GameId);

            return _playedGames.Where(isInRecentGames).ToList();
        }

        /// <summary>
        /// Adds one player of a game that already finished, so the game appears in the recent games.
        /// </summary>
        /// <param name="record">What the player scored in the game.</param>
        public void AddPlayedGameRecord(PlayedGameRecord record)
        {
            _playedGames.Add(record);
        }

        /// <inheritdoc />
        public List<PlayerRankingRecord> GetRanking()
        {
            return _rankingRecords.ToList();
        }

        /// <summary>
        /// Adds the totals of a player, so the player appears in the ranking.
        /// </summary>
        /// <param name="record">Totals of the player.</param>
        public void AddRankingRecord(PlayerRankingRecord record)
        {
            _rankingRecords.Add(record);
        }

        /// <inheritdoc />
        public void Save(GameResultRecord result)
        {
            _savedResults.Add(result);
        }
    }
}
