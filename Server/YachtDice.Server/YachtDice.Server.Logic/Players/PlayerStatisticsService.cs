using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Calculates the statistics of the players from their game history.
    /// </summary>
    public class PlayerStatisticsService
    {
        private const int DefaultPlayerLevel = 1;
        private const int EmptyCount = 0;
        private const int PercentageFactor = 100;
        private const string NullDependencyMessage = "The dependency cannot be null.";

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
            if (playerRepository == null)
            {
                throw new ArgumentNullException(nameof(playerRepository), NullDependencyMessage);
            }

            if (gameHistoryRepository == null)
            {
                throw new ArgumentNullException(nameof(gameHistoryRepository), NullDependencyMessage);
            }

            _playerRepository = playerRepository;
            _gameHistoryRepository = gameHistoryRepository;
        }

        /// <summary>
        /// Gets the statistics of the player with the given display name or username.
        /// </summary>
        /// <param name="playerName">Display name or username of the player.</param>
        /// <returns>
        /// The statistics; <see cref="PlayerStatistics.IsPlayerFound"/> is false if the player does not exist.
        /// </returns>
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

        private static PlayerStatistics BuildStatistics(Player player, List<GameHistoryPlayer> history)
        {
            int gamesPlayed = history.Count;
            int wins = history.Count(entry => entry.IsWinner == true);
            int bestScore = history.Any() ? history.Max(entry => entry.FinalScore) : EmptyCount;
            int winRate = gamesPlayed > EmptyCount
                ? (int)Math.Round((double)wins * PercentageFactor / gamesPlayed)
                : EmptyCount;

            return new PlayerStatistics
            {
                IsPlayerFound = true,
                GamesPlayed = gamesPlayed,
                Wins = wins,
                BestScore = bestScore,
                WinRatePercentage = winRate,
                Level = player.Level ?? DefaultPlayerLevel
            };
        }
    }
}