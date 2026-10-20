namespace YachtDice.Server.Services.Mapping
{
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
            return new Contracts.PlayerStatistics
            {
                IsPlayerFound = statistics.IsPlayerFound,
                GamesPlayed = statistics.GamesPlayed,
                Wins = statistics.Wins,
                BestScore = statistics.BestScore,
                WinRatePercentage = statistics.WinRatePercentage,
                Level = statistics.Level
            };
        }
    }
}