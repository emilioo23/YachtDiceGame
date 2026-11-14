using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Services.Mapping
{
    using Contracts = YachtDice.Server.Contracts.Players;
    using Logic = YachtDice.Server.Logic;

    /// <summary>
    /// Converts the player statistics of the logic layer into the data contract.
    /// </summary>
    internal static class PlayerStatisticsMapper
    {
        /// <summary>
        /// Creates the data contract that is sent back to the client.
        /// </summary>
        /// <param name="statistics">Statistics produced by the logic layer.</param>
        /// <returns>Answer for the client.</returns>
        public static Contracts.PlayerStatistics ToContract(Logic.Players.PlayerStatistics statistics)
        {
            var contract = new Contracts.PlayerStatistics
            {
                GamesPlayed = statistics.GamesPlayed,
                Wins = statistics.Wins,
                BestScore = statistics.BestScore,
                WinRatePercentage = statistics.WinRatePercentage,
                Level = statistics.Level,
                Experience = statistics.Experience
            };

            return contract;
        }

        /// <summary>
        /// Converts the recent games of a player into the version that travels to the clients.
        /// </summary>
        /// <param name="games">Games of the logic layer.</param>
        /// <returns>The games for the clients.</returns>
        public static List<Contracts.GameHistoryEntry> ToHistoryContracts(List<Logic.Players.PlayedGameSummary> games)
        {
            Func<Logic.Players.PlayedGameSummary, Contracts.GameHistoryEntry> createEntry = ToHistoryContract;

            return games.Select(createEntry).ToList();
        }

        private static Contracts.GameHistoryEntry ToHistoryContract(Logic.Players.PlayedGameSummary game)
        {
            var contract = new Contracts.GameHistoryEntry
            {
                PlayedAt = game.PlayedAt,
                IsWin = game.IsWin,
                OwnScore = game.OwnScore,
                BestOpponentScore = game.BestOpponentScore,
                Opponents = game.Opponents
            };

            return contract;
        }
    }
}