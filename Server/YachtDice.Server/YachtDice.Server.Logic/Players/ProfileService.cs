using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Exceptions;
using YachtDice.Server.Data.Logging;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Moderation;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Changes the data of the profile that the players choose themselves.
    /// </summary>
    public class ProfileService
    {
        private const int DaysBetweenUsernameChanges = 30;
        private const int FlagCodeLength = 2;
        private const char FirstLetter = 'a';
        private const char LastLetter = 'z';

        private readonly AppLogger _logger = new AppLogger(typeof(ProfileService));
        private readonly IPlayerRepository _playerRepository;
        private readonly OffensiveWordFilter _wordFilter;
        private readonly PlayerAchievements _achievements;

        /// <summary>
        /// Initializes the service with the data access and the filter it needs.
        /// </summary>
        /// <param name="playerRepository">Data access of the players.</param>
        /// <param name="wordFilter">Filter that detects offensive words.</param>
        /// <param name="achievements">Achievements and titles that each player has unlocked.</param>
        /// <exception cref="ArgumentNullException">Any of the parameters is null.</exception>
        public ProfileService(
            IPlayerRepository playerRepository,
            OffensiveWordFilter wordFilter,
            PlayerAchievements achievements)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), LogMessages.NullPlayerRepositoryMessage);
            _wordFilter = wordFilter ?? throw new ArgumentNullException(nameof(wordFilter),
                LogMessages.NullWordFilterMessage);
            _achievements = achievements
                ?? throw new ArgumentNullException(nameof(achievements), LogMessages.NullAchievementsMessage);
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
            Player player = FindPlayers(currentName).FirstOrDefault();
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
            return _achievements.GetStatuses(playerName);
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
            Player player = FindPlayers(playerName).FirstOrDefault();
            TitleChangeStatus status = TitleChangeStatus.PlayerNotFound;

            if (player != null)
            {
                status = _achievements.CheckTitle(playerName, titleId);
            }

            if (status == TitleChangeStatus.Success)
            {
                _playerRepository.SaveTitle(player.Id, titleId.ToString());
            }

            return status;
        }

        /// <summary>
        /// Saves the country flag that a player wants to show next to the avatar. Any country can be chosen.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="flagCode">Code of the country with two letters, or an empty text to remove the flag.</param>
        /// <returns>The result of the change.</returns>
        /// <exception cref="DataAccessException">The flag could not be saved.</exception>
        public FlagChangeStatus ChangeFlag(string playerName, string flagCode)
        {
            Player player = FindPlayers(playerName).FirstOrDefault();
            string code = flagCode?.Trim().ToLowerInvariant() ?? string.Empty;
            FlagChangeStatus status = FlagChangeStatus.PlayerNotFound;

            if (player != null)
            {
                status = IsValidFlagCode(code) ? FlagChangeStatus.Success : FlagChangeStatus.InvalidFlag;
            }

            if (status == FlagChangeStatus.Success)
            {
                _playerRepository.SaveFlagCode(player.Id, code);
            }

            return status;
        }

        /// <summary>
        /// Deletes the account of a player with all its data: profile, security settings, friendships, scorecards
        /// and game history. It cannot be undone.
        /// </summary>
        /// <param name="playerName">Name of the player whose account is deleted.</param>
        /// <param name="password">Password that the player typed to confirm the deletion.</param>
        /// <returns>The result of the deletion.</returns>
        /// <exception cref="DataAccessException">The account could not be deleted.</exception>
        public AccountDeletionStatus DeleteAccount(string playerName, string password)
        {
            Player player = FindPlayers(playerName).FirstOrDefault();
            AccountDeletionStatus status = AccountDeletionStatus.PlayerNotFound;

            if (player != null && !CredentialRules.Verify(password ?? string.Empty, player.PwdHash))
            {
                _logger.Warning(string.Format(CultureInfo.InvariantCulture, LogMessages.ProfileWrongPasswordLogFormat,
                    player.Id));
                status = AccountDeletionStatus.WrongPassword;
            }
            else if (player != null)
            {
                _playerRepository.Delete(player.Id);
                _logger.Info(string.Format(CultureInfo.InvariantCulture, LogMessages.DeletedLogFormat, player.Id));
                status = AccountDeletionStatus.Success;
            }

            return status;
        }

        private static bool IsValidFlagCode(string code)
        {
            Func<char, bool> isLetter = letter => letter >= FirstLetter && letter <= LastLetter;

            return code.Length == 0 || (code.Length == FlagCodeLength && code.All(isLetter));
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
            else if (username != player.Username && HasChangedRecently(player))
            {
                status = UsernameChangeStatus.ChangedRecently;
            }

            return status;
        }

        private static bool HasChangedRecently(Player player)
        {
            DateTime oldestAllowedChange = DateTime.UtcNow.AddDays(-DaysBetweenUsernameChanges);

            return player.LastUserChange.HasValue && player.LastUserChange.Value > oldestAllowedChange;
        }

        private bool IsUsedByAnotherPlayer(Player player, string username)
        {
            List<Player> owners = _playerRepository.FindByUsername(username);

            Func<Player, bool> isAnotherPlayer = owner => owner.Id != player.Id;

            return owners.Any(isAnotherPlayer);
        }

        private List<Player> FindPlayers(string name)
        {
            var matches = new List<Player>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                matches = _playerRepository.FindByName(name);
            }

            return matches;
        }
    }
}
