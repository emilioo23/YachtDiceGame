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
        private const string NullDependencyMessage = "A dependency of the player statistics service cannot be null.";

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
                ?? throw new ArgumentNullException(nameof(playerRepository), NullDependencyMessage);
            _gameHistoryRepository = gameHistoryRepository
                ?? throw new ArgumentNullException(nameof(gameHistoryRepository), NullDependencyMessage);
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
                List<GameHistoryPlayer> computerGames = _gameHistoryRepository.FindComputerGames(player.Id);
                statistics = BuildStatistics(player, history);
                statistics.GamesAgainstComputer = computerGames.Count;
                statistics.WinsAgainstComputer = CountWins(computerGames);
            }

            return statistics;
        }

        /// <summary>
        /// Gets the players with the most points, from the first place downwards. A tie in points is broken by
        /// the wins, and players who also have the same wins share the position. The player who asks always sees
        /// the own position: when it is not among the best players, it is added at the end.
        /// </summary>
        /// <param name="viewerName">Name of the player who looks at the leaderboard.</param>
        /// <returns>The entries of the leaderboard, in order.</returns>
        /// <exception cref="DataAccessException">The ranking could not be read.</exception>
        public List<LeaderboardEntry> GetTopPlayers(string viewerName)
        {
            List<LeaderboardEntry> ranking = AssignPositions(_gameHistoryRepository.GetRanking());
            List<LeaderboardEntry> visibleEntries = ranking.Take(MaxLeaderboardEntries).ToList();
            Func<LeaderboardEntry, bool> isViewer =
                leaderboardEntry => string.Equals(
                    leaderboardEntry.DisplayName,
                    viewerName,
                    StringComparison.OrdinalIgnoreCase);
            List<LeaderboardEntry> viewerOutsideTop = ranking.Skip(MaxLeaderboardEntries).Where(isViewer).ToList();

            visibleEntries.AddRange(viewerOutsideTop);

            return visibleEntries;
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

        private static List<LeaderboardEntry> AssignPositions(List<PlayerRankingRecord> records)
        {
            List<PlayerRankingRecord> ordered = records
                .OrderByDescending(record => record.TotalPoints)
                .ThenByDescending(record => record.Wins)
                .ThenBy(record => record.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var entries = new List<LeaderboardEntry>();
            int position = FirstPosition;

            for (int index = 0; index < ordered.Count; index++)
            {
                bool isTiedWithPrevious = index > 0 && IsTied(ordered[index - 1], ordered[index]);

                if (!isTiedWithPrevious)
                {
                    position = index + FirstPosition;
                }

                entries.Add(CreateLeaderboardEntry(ordered[index], position));
            }

            return entries;
        }

        private static bool IsTied(PlayerRankingRecord previous, PlayerRankingRecord current)
        {
            return previous.TotalPoints == current.TotalPoints && previous.Wins == current.Wins;
        }

        private static LeaderboardEntry CreateLeaderboardEntry(PlayerRankingRecord record, int position)
        {
            var leaderboardEntry = new LeaderboardEntry
            {
                Position = position,
                DisplayName = record.DisplayName,
                AvatarId = record.AvatarId,
                TotalPoints = record.TotalPoints,
                Wins = record.Wins,
                GamesPlayed = record.GamesPlayed,
                Level = record.Level
            };

            return leaderboardEntry;
        }

        private static PlayerStatistics BuildStatistics(Player player, List<GameHistoryPlayer> history)
        {
            int gamesPlayed = history.Count;
            int wins = CountWins(history);

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

        private static int CountWins(List<GameHistoryPlayer> history)
        {
            Func<GameHistoryPlayer, bool> isWinner = historyPlayer => historyPlayer.IsWinner == true;

            return history.Count(isWinner);
        }

        private static int FindBestScore(List<GameHistoryPlayer> history)
        {
            int bestScore = EmptyCount;

            if (history.Count > EmptyCount)
            {
                bestScore = history.Max(historyPlayer => historyPlayer.FinalScore);
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