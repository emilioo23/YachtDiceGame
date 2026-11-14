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
        private static int GetExperienceToReach(int level)
        {
            long requiredExperience = PlayerProgression.GetRequiredExperience(level);

            return (int)Math.Min(requiredExperience, int.MaxValue);
        }

        private const int DefaultLevel = 1;
        private const int DefaultExperience = 0;

        private readonly IPlayerRepository _playerRepository;

        /// <summary>
        /// Initializes the service with the data access it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read and save the players.</param>
        public AvatarService(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository));
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
        /// Gets the avatar, level and experience of a player, with the experience that marks the start of the
        /// current level and the start of the next one.
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
                Experience = DefaultExperience
            };

            if (player != null)
            {
                profile.AvatarId = player.AvatarId;
                profile.Level = player.Level ?? DefaultLevel;
                profile.Experience = player.XP ?? DefaultExperience;
            }

            profile.LevelStartExperience = GetExperienceToReach(profile.Level);
            profile.NextLevelExperience = GetExperienceToReach(profile.Level + 1);

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
