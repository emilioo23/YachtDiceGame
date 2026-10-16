using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Services.Contracts;

namespace YachtDice.Server.Services.Mapping
{
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
        public static List<LeaderboardEntryDto> ToDtos(List<LeaderboardEntry> entries)
        {
            return entries
                .Select(entry => new LeaderboardEntryDto
                {
                    Position = entry.Position,
                    DisplayName = entry.DisplayName,
                    TotalPoints = entry.TotalPoints,
                    Wins = entry.Wins
                })
                .ToList();
        }
    }
}