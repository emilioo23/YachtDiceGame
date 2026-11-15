using System;
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
            Func<Logic.Players.LeaderboardEntry, Contracts.LeaderboardEntry> createLeaderboardEntry = ToContract;
            List<Contracts.LeaderboardEntry> contracts = entries
                .Select(createLeaderboardEntry)
                .ToList();

            return contracts;
        }

        /// <summary>
        /// Converts a row of the leaderboard into the version that travels to the clients.
        /// </summary>
        /// <param name="leaderboardEntry">Row of the leaderboard in the logic layer.</param>
        /// <returns>The row for the clients.</returns>
        private static Contracts.LeaderboardEntry ToContract(Logic.Players.LeaderboardEntry leaderboardEntry)
        {
            var contract = new Contracts.LeaderboardEntry
            {
                Position = leaderboardEntry.Position,
                DisplayName = leaderboardEntry.DisplayName,
                AvatarId = leaderboardEntry.AvatarId,
                TotalPoints = leaderboardEntry.TotalPoints,
                Wins = leaderboardEntry.Wins,
                GamesPlayed = leaderboardEntry.GamesPlayed,
                Level = leaderboardEntry.Level
            };

            return contract;
        }
    }
}