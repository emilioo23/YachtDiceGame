using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Exceptions;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Moderation;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Changes the data of the profile that the players choose themselves.
    /// </summary>
    public class ProfileService
    {
        private const string NullRepositoryMessage = "The data access of the players cannot be null.";
        private const string NullWordFilterMessage = "The filter of offensive words cannot be null.";
        private const string NullStatisticsMessage = "The service of statistics cannot be null.";

        private readonly IPlayerRepository _playerRepository;
        private readonly OffensiveWordFilter _wordFilter;
        private readonly PlayerStatisticsService _statisticsService;

        /// <summary>
        /// Initializes the service with the data access and the filter it needs.
        /// </summary>
        /// <param name="playerRepository">Data access of the players.</param>
        /// <param name="wordFilter">Filter that detects offensive words.</param>
        /// <param name="statisticsService">Service that calculates the statistics that unlock achievements.</param>
        /// <exception cref="ArgumentNullException">Any of the parameters is null.</exception>
        public ProfileService(
            IPlayerRepository playerRepository,
            OffensiveWordFilter wordFilter,
            PlayerStatisticsService statisticsService)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), NullRepositoryMessage);
            _wordFilter = wordFilter ?? throw new ArgumentNullException(nameof(wordFilter), NullWordFilterMessage);
            _statisticsService = statisticsService
                ?? throw new ArgumentNullException(nameof(statisticsService), NullStatisticsMessage);
        }

        /// <summary>
        /// Changes the username of a player, with the same rules that the registration uses.
        /// </summary>
        /// <param name="currentName">Name that the player uses now.</param>
        /// <param name="newUsername">Username that the player wants.</param>
        /// <returns>The result of the change.</returns>
        /// <exception cref="DataAccessException">The new username could not be saved.</exception>
        public UsernameChangeStatus ChangeUsername(string currentName, string newUsername)
        {
            Player player = FindPlayer(currentName);
            string username = newUsername?.Trim() ?? string.Empty;
            UsernameChangeStatus status = UsernameChangeStatus.PlayerNotFound;

            if (player != null)
            {
                status = FindUsernameProblem(player, username);
            }

            if (status == UsernameChangeStatus.Success && username != player.Username)
            {
                _playerRepository.SaveUsername(player.Id, username);
            }

            return status;
        }

        /// <summary>
        /// Gets every achievement with the information of whether the player has unlocked it.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>The achievements, in the order in which they are shown.</returns>
        /// <exception cref="DataAccessException">The statistics could not be read.</exception>
        public List<AchievementStatus> GetAchievements(string playerName)
        {
            List<int> unlockedIds = AchievementRules.GetUnlockedIds(_statisticsService.GetStatistics(playerName));
            var achievements = new List<AchievementStatus>();

            foreach (AchievementRule rule in AchievementRules.GetAll())
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
        /// Saves the title that a player wants to show in the profile, if the player has unlocked it.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="titleId">Identifier of the title.</param>
        /// <returns>The result of the change.</returns>
        /// <exception cref="DataAccessException">The title could not be saved.</exception>
        public TitleChangeStatus ChangeTitle(string playerName, int titleId)
        {
            Player player = FindPlayer(playerName);
            TitleChangeStatus status = TitleChangeStatus.PlayerNotFound;

            if (player != null)
            {
                status = FindTitleProblem(playerName, titleId);
            }

            if (status == TitleChangeStatus.Success)
            {
                _playerRepository.SaveTitle(player.Id, titleId.ToString());
            }

            return status;
        }

        private TitleChangeStatus FindTitleProblem(string playerName, int titleId)
        {
            List<int> unlockedIds = AchievementRules.GetUnlockedIds(_statisticsService.GetStatistics(playerName));
            TitleChangeStatus status = TitleChangeStatus.Success;

            if (!AchievementRules.ExistsTitle(titleId))
            {
                status = TitleChangeStatus.TitleNotFound;
            }
            else if (!AchievementRules.IsTitleAvailable(titleId, unlockedIds))
            {
                status = TitleChangeStatus.TitleLocked;
            }

            return status;
        }

        private UsernameChangeStatus FindUsernameProblem(Player player, string username)
        {
            UsernameChangeStatus status = UsernameChangeStatus.Success;

            if (!CredentialRules.IsValidUsername(username))
            {
                status = UsernameChangeStatus.InvalidUsername;
            }
            else if (_wordFilter.ContainsOffensiveWord(username))
            {
                status = UsernameChangeStatus.OffensiveUsername;
            }
            else if (IsUsedByAnotherPlayer(player, username))
            {
                status = UsernameChangeStatus.UsernameAlreadyTaken;
            }

            return status;
        }

        private bool IsUsedByAnotherPlayer(Player player, string username)
        {
            List<Player> owners = _playerRepository.FindByUsername(username);

            Func<Player, bool> isAnotherPlayer = owner => owner.Id != player.Id;

            return owners.Any(isAnotherPlayer);
        }

        private Player FindPlayer(string name)
        {
            var matches = new List<Player>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                matches = _playerRepository.FindByName(name);
            }

            return matches.FirstOrDefault();
        }
    }
}
