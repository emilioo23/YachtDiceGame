using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Players;

namespace YachtDice.Server.Logic.Avatars
{
    /// <summary>
    /// Lets the players see their avatars and choose among the ones they have unlocked.
    /// </summary>
    public class AvatarService : IAvatarLookup
    {
        private const int DefaultLevel = 1;
        private const int DefaultExperience = 0;
        private const string NullPlayerRepositoryMessage = "The data access of the players cannot be null.";
        private const string NullLevelTableMessage = "The table of levels cannot be null.";

        private readonly IPlayerRepository _playerRepository;
        private readonly LevelTable _levelTable;

        /// <summary>
        /// Initializes the service with the data access and the table of levels it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read and save the players.</param>
        /// <param name="levelTable">Experience that each level needs.</param>
        public AvatarService(IPlayerRepository playerRepository, LevelTable levelTable)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), NullPlayerRepositoryMessage);
            _levelTable = levelTable ?? throw new ArgumentNullException(nameof(levelTable), NullLevelTableMessage);
        }

        /// <summary>
        /// Gets the avatar of the player who owns the name.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The identifier of the avatar, or the default avatar if the player is not registered.</returns>
        public int GetAvatarId(string displayName)
        {
            Player player = FindPlayer(displayName);

            return player == null ? AvatarCatalog.DefaultAvatarId : player.AvatarId;
        }

        /// <summary>
        /// Gets the avatar, level and experience of a player, with the experience where the level starts and
        /// where the next one starts.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The profile; a profile with the default values if the player does not exist.</returns>
        public PlayerProfile GetProfile(string displayName)
        {
            Player player = FindPlayer(displayName);
            var profile = new PlayerProfile
            {
                AvatarId = AvatarCatalog.DefaultAvatarId,
                Level = DefaultLevel,
                Experience = DefaultExperience,
                TitleId = AchievementRules.DefaultTitleId,
                FlagCode = string.Empty
            };

            if (player != null)
            {
                profile.AvatarId = player.AvatarId;
                profile.Level = player.Level ?? DefaultLevel;
                profile.Experience = player.XP ?? DefaultExperience;
                profile.TitleId = ReadTitleId(player.Id);
                profile.FlagCode = _playerRepository.FindFlagCode(player.Id);
            }

            profile.LevelStartExperience = _levelTable.GetLevelStart(profile.Level);
            profile.NextLevelExperience = _levelTable.GetNextLevelStart(profile.Level);

            return profile;
        }

        /// <summary>
        /// Changes the avatar of a player, only if the avatar exists and the player has unlocked it.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="avatarId">Identifier of the avatar that the player wants.</param>
        /// <returns>The result of the change.</returns>
        public AvatarChangeStatus ChangeAvatar(string displayName, int avatarId)
        {
            Player player = FindPlayer(displayName);
            AvatarChangeStatus status = AvatarChangeStatus.PlayerNotFound;

            if (player != null)
            {
                status = GetChangeStatus(player, avatarId);
            }

            if (status == AvatarChangeStatus.Success)
            {
                _playerRepository.SaveAvatar(player.Id, avatarId);
            }

            return status;
        }

        private int ReadTitleId(int playerId)
        {
            bool isNumber = int.TryParse(_playerRepository.FindTitle(playerId), out int titleId);

            return isNumber ? titleId : AchievementRules.DefaultTitleId;
        }

        private static AvatarChangeStatus GetChangeStatus(Player player, int avatarId)
        {
            AvatarChangeStatus status = AvatarChangeStatus.Success;

            if (!AvatarCatalog.TryFind(avatarId, out UnlockableAvatar avatar))
            {
                status = AvatarChangeStatus.AvatarNotFound;
            }
            else if ((player.Level ?? DefaultLevel) < avatar.RequiredLevel)
            {
                status = AvatarChangeStatus.AvatarLocked;
            }

            return status;
        }

        private Player FindPlayer(string displayName)
        {
            var matches = new List<Player>();

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                matches = _playerRepository.FindByName(displayName);
            }

            return matches.FirstOrDefault();
        }
    }
}
