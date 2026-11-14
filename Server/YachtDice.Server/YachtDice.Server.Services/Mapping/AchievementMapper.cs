using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Services.Mapping
{
    using Contracts = YachtDice.Server.Contracts.Players;
    using Logic = YachtDice.Server.Logic;

    /// <summary>
    /// Converts the achievements of the logic layer into the data contracts.
    /// </summary>
    internal static class AchievementMapper
    {
        /// <summary>
        /// Converts the achievements of a player into the version that travels to the clients.
        /// </summary>
        /// <param name="achievements">Achievements of the logic layer.</param>
        /// <returns>The achievements for the clients.</returns>
        public static List<Contracts.AchievementInfo> ToContracts(List<Logic.Players.AchievementStatus> achievements)
        {
            Func<Logic.Players.AchievementStatus, Contracts.AchievementInfo> createInfo = ToContract;
            List<Contracts.AchievementInfo> contracts = achievements
                .Select(createInfo)
                .ToList();

            return contracts;
        }

        private static Contracts.AchievementInfo ToContract(Logic.Players.AchievementStatus achievement)
        {
            var contract = new Contracts.AchievementInfo
            {
                Id = achievement.Id,
                IsUnlocked = achievement.IsUnlocked,
                HasTitle = achievement.HasTitle
            };

            return contract;
        }
    }
}
