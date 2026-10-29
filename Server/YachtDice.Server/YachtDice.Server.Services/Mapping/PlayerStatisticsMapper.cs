namespace YachtDice.Server.Services.Mapping
{
    using Contracts = YachtDice.Server.Contracts;
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
            Contracts.PlayerStatistics result;

            if (statistics == null)
            {
                result = new Contracts.PlayerStatistics();
            }
            else
            {
                result = new Contracts.PlayerStatistics
                {
                    GamesPlayed = statistics.GamesPlayed,
                    Wins = statistics.Wins,
                    BestScore = statistics.BestScore,
                    WinRatePercentage = statistics.WinRatePercentage,
                    Level = statistics.Level
                };
            }

            return result;
        }
    }
}