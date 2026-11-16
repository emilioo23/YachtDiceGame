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
                Experience = statistics.Experience,
                GamesAgainstComputer = statistics.GamesAgainstComputer,
                WinsAgainstComputer = statistics.WinsAgainstComputer
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

        /// <summary>
        /// Converts the scorecards of a past game into the objects that travel to the clients.
        /// </summary>
        /// <param name="scorecards">Scorecards of the logic layer.</param>
        /// <returns>The scorecards for the clients.</returns>
        public static List<Contracts.SavedScorecard> ToScorecardContracts(List<Logic.Players.SavedScorecard> scorecards)
        {
            Func<Logic.Players.SavedScorecard, Contracts.SavedScorecard> createScorecard = ToScorecardContract;

            return scorecards.Select(createScorecard).ToList();
        }

        private static Contracts.SavedScorecard ToScorecardContract(Logic.Players.SavedScorecard scorecard)
        {
            Func<Logic.Players.SavedScore, Contracts.SavedScore> createScore = score => new Contracts.SavedScore
            {
                Category = score.Category,
                Points = score.Points,
                IsUsed = score.IsUsed
            };

            return new Contracts.SavedScorecard
            {
                PlayerName = scorecard.PlayerName,
                Total = scorecard.Total,
                HasUpperBonus = scorecard.HasUpperBonus,
                YachtBonusCount = scorecard.YachtBonusCount,
                Scores = scorecard.Scores.Select(createScore).ToList()
            };
        }

        private static Contracts.GameHistoryEntry ToHistoryContract(Logic.Players.PlayedGameSummary game)
        {
            var contract = new Contracts.GameHistoryEntry
            {
                PlayedAt = game.PlayedAt,
                GameId = game.GameId,
                IsWin = game.IsWin,
                OwnScore = game.OwnScore,
                BestOpponentScore = game.BestOpponentScore,
                Opponents = game.Opponents,
                IsAgainstComputer = game.IsAgainstComputer
            };

            return contract;
        }
    }
}