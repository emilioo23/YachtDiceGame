using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Services.Mapping
{
    using Contracts = YachtDice.Server.Contracts.Players;
    using Logic = YachtDice.Server.Logic;

    /// <summary>
    /// Converts the ranking of the logic layer into data contracts.
    /// </summary>
    internal static class LeaderboardMapper
    {
        /// <summary>
        /// Creates the data contracts that are sent back to the client.
        /// </summary>
        /// <param name="entries">Ranking produced by the logic layer.</param>
        /// <returns>Answer for the client, in the same order.</returns>
        public static List<Contracts.LeaderboardEntry> ToContracts(List<Logic.Players.LeaderboardEntry> entries)
        {
            List<Contracts.LeaderboardEntry> contracts = entries.Select(ToContract).ToList();

            return contracts;
        }

        private static Contracts.LeaderboardEntry ToContract(Logic.Players.LeaderboardEntry entry)
        {
            var contract = new Contracts.LeaderboardEntry
            {
                Position = entry.Position,
                DisplayName = entry.DisplayName,
                TotalPoints = entry.TotalPoints,
                Wins = entry.Wins
            };

            return contract;
        }
    }
}