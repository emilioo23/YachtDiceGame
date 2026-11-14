using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Calculates the statistics and the leaderboard of the players from their game history.
    /// </summary>
    public class PlayerStatisticsService
    {
        private const int DefaultPlayerLevel = 1;
        private const int EmptyCount = 0;
        private const int PercentageFactor = 100;
        private const int FirstPosition = 1;
        private const int MaxLeaderboardEntries = 100;
        private const int MaxHistoryGames = 20;
        private const string OpponentSeparator = ", ";

        private readonly IPlayerRepository _playerRepository;
        private readonly IGameHistoryRepository _gameHistoryRepository;

        /// <summary>
        /// Initializes the service with the data access it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read players.</param>
        /// <param name="gameHistoryRepository">Repository used to read the game results.</param>
        public PlayerStatisticsService(
            IPlayerRepository playerRepository,
            IGameHistoryRepository gameHistoryRepository)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository));
            _gameHistoryRepository = gameHistoryRepository
                ?? throw new ArgumentNullException(nameof(gameHistoryRepository));
        }

        /// <summary>
        /// Gets the statistics of the player with the given display name or username.
        /// </summary>
        /// <param name="playerName">Display name or username of the player.</param>
        /// <returns>The statistics; they are empty if the player does not exist.</returns>
        public PlayerStatistics GetStatistics(string playerName)
        {
            List<Player> matches = _playerRepository.FindByName(playerName);
            Player player = matches.FirstOrDefault();
            var statistics = new PlayerStatistics();

            if (player != null)
            {
                List<GameHistoryPlayer> history = _gameHistoryRepository.FindByPlayerId(player.Id);
                statistics = BuildStatistics(player, history);
            }

            return statistics;
        }

        /// <summary>
        /// Gets the players with the most points, from the first place downwards.
        /// </summary>
        /// <returns>The entries of the leaderboard, in order.</returns>
        /// <exception cref="DataAccessException">The ranking could not be read.</exception>
        public List<LeaderboardEntry> GetTopPlayers()
        {
            List<PlayerRankingRecord> records = _gameHistoryRepository.GetRanking(MaxLeaderboardEntries);
            Func<PlayerRankingRecord, int, LeaderboardEntry> createLeaderboardEntry = CreateLeaderboardEntry;

            List<LeaderboardEntry> entries = records
                .Select(createLeaderboardEntry)
                .ToList();

            return entries;
        }

        /// <summary>
        /// Gets the most recent games of a player, from the newest to the oldest.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>The games of the player, or an empty list if the player does not exist or has no games.</returns>
        /// <exception cref="DataAccessException">The history could not be read.</exception>
        public List<PlayedGameSummary> GetRecentGames(string playerName)
        {
            Player player = _playerRepository.FindByName(playerName).FirstOrDefault();
            var games = new List<PlayedGameSummary>();

            if (player != null)
            {
                List<PlayedGameRecord> records = _gameHistoryRepository.FindRecentGames(player.Id, MaxHistoryGames);
                Func<PlayedGameRecord, int> readGameId = record => record.GameId;

                foreach (IGrouping<int, PlayedGameRecord> game in records.GroupBy(readGameId))
                {
                    games.Add(SummarizeGame(player.Id, game.ToList()));
                }
            }

            Func<PlayedGameSummary, DateTime> readPlayedAt = summary => summary.PlayedAt;

            return games.OrderByDescending(readPlayedAt).ToList();
        }

        private static PlayedGameSummary SummarizeGame(int playerId, List<PlayedGameRecord> game)
        {
            Func<PlayedGameRecord, bool> isOwnRecord = record => record.PlayerId == playerId;
            Func<PlayedGameRecord, bool> isOpponent = record => record.PlayerId != playerId;
            Func<PlayedGameRecord, string> readName = record => record.PlayerName;
            Func<PlayedGameRecord, int> readScore = record => record.FinalScore;
            PlayedGameRecord own = game.First(isOwnRecord);
            List<PlayedGameRecord> opponents = game.Where(isOpponent).ToList();

            var summary = new PlayedGameSummary
            {
                PlayedAt = own.EndDate ?? DateTime.MinValue,
                IsWin = own.IsWinner,
                OwnScore = own.FinalScore,
                BestOpponentScore = opponents.Count > EmptyCount ? opponents.Max(readScore) : EmptyCount,
                Opponents = string.Join(OpponentSeparator, opponents.Select(readName))
            };

            return summary;
        }

        private static LeaderboardEntry CreateLeaderboardEntry(PlayerRankingRecord record, int index)
        {
            var entry = new LeaderboardEntry
            {
                Position = index + FirstPosition,
                DisplayName = record.DisplayName,
                AvatarId = record.AvatarId,
                TotalPoints = record.TotalPoints,
                Wins = record.Wins
            };

            return entry;
        }

        private static PlayerStatistics BuildStatistics(Player player, List<GameHistoryPlayer> history)
        {
            int gamesPlayed = history.Count;
            Func<GameHistoryPlayer, bool> isWinner = entry => entry.IsWinner == true;
            int wins = history.Count(isWinner);

            var statistics = new PlayerStatistics
            {
                GamesPlayed = gamesPlayed,
                Wins = wins,
                BestScore = FindBestScore(history),
                WinRatePercentage = CalculateWinRate(wins, gamesPlayed),
                Level = player.Level ?? DefaultPlayerLevel,
                Experience = player.XP ?? EmptyCount
            };

            return statistics;
        }

        private static int FindBestScore(List<GameHistoryPlayer> history)
        {
            int bestScore = EmptyCount;

            if (history.Count > EmptyCount)
            {
                bestScore = history.Max(entry => entry.FinalScore);
            }

            return bestScore;
        }

        private static int CalculateWinRate(int wins, int gamesPlayed)
        {
            int winRate = EmptyCount;

            if (gamesPlayed > EmptyCount)
            {
                winRate = (int)Math.Round((double)wins * PercentageFactor / gamesPlayed);
            }

            return winRate;
        }
    }
}