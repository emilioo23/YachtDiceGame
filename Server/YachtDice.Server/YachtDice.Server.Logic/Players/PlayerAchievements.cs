using System;
using System.Collections.Generic;
using YachtDice.Server.Data.Exceptions;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Joins the statistics of a player with the achievements of the game to know which achievements and titles
    /// the player has unlocked.
    /// </summary>
    public sealed class PlayerAchievements
    {
        private const string NullDependencyMessage = "A dependency of the achievements of the players cannot be null.";

        private readonly PlayerStatisticsService _statisticsService;
        private readonly AchievementCatalog _catalog;

        /// <summary>
        /// Initializes the achievements of the players with the services they need.
        /// </summary>
        /// <param name="statisticsService">Service that calculates the statistics that unlock achievements.</param>
        /// <param name="catalog">Achievements of the game.</param>
        /// <exception cref="ArgumentNullException">Any of the parameters is null.</exception>
        public PlayerAchievements(PlayerStatisticsService statisticsService, AchievementCatalog catalog)
        {
            _statisticsService = statisticsService
                ?? throw new ArgumentNullException(nameof(statisticsService), NullDependencyMessage);
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog), NullDependencyMessage);
        }

        /// <summary>
        /// Gets every achievement with the information of whether the player has unlocked it.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>The achievements, in the order in which they are shown.</returns>
        /// <exception cref="DataAccessException">The statistics could not be read.</exception>
        public List<AchievementStatus> GetStatuses(string playerName)
        {
            List<int> unlockedIds = GetUnlockedIds(playerName);
            var achievements = new List<AchievementStatus>();

            foreach (AchievementRule rule in _catalog.GetAll())
            {
                achievements.Add(new AchievementStatus
                {
                    Id = rule.Id,
                    IsUnlocked = unlockedIds.Contains(rule.Id),
                    HasTitle = rule.HasTitle
                });
            }

            return achievements;
        }

        /// <summary>
        /// Checks whether the player can show a title.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="titleId">Identifier of the title.</param>
        /// <returns>Success, or why the player cannot show the title.</returns>
        /// <exception cref="DataAccessException">The statistics could not be read.</exception>
        public TitleChangeStatus CheckTitle(string playerName, int titleId)
        {
            TitleChangeStatus status = TitleChangeStatus.Success;

            if (!_catalog.ExistsTitle(titleId))
            {
                status = TitleChangeStatus.TitleNotFound;
            }
            else if (!_catalog.IsTitleAvailable(titleId, GetUnlockedIds(playerName)))
            {
                status = TitleChangeStatus.TitleLocked;
            }

            return status;
        }

        private List<int> GetUnlockedIds(string playerName)
        {
            return _catalog.GetUnlockedIds(_statisticsService.GetStatistics(playerName));
        }
    }
}
