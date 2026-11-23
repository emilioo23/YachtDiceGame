using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Players;

namespace YachtDice.Server.Logic.Avatars
{
    /// <summary>
    /// Avatars of the players: the catalog of unlockable avatars, the level of a player and the change of avatar.
    /// </summary>
    public class AvatarService : IAvatarLookup
    {
        /// <summary>
        /// Identifier of the avatar that every new player has.
        /// </summary>
        public const int DefaultAvatarId = FirstAvatarId;

        private const int GuestLevel = 0;
        private const int DefaultLevel = 1;
        private const int DefaultExperience = 0;
        private const string NullPlayerRepositoryMessage = "The data access of the players cannot be null.";
        private const string NullLevelTableMessage = "The table of levels cannot be null.";
        private const int FirstAvatarId = 1;
        private const int FreeLevel = 1;
        private const int RookieLevel = 2;
        private const int BronzeLevel = 3;
        private const int ApprenticeLevel = 4;
        private const int SilverLevel = 5;
        private const int SkilledLevel = 6;
        private const int VeteranLevel = 7;
        private const int GoldLevel = 8;
        private const int ExpertLevel = 9;
        private const int MasterLevel = 10;
        private const int ElderLevel = 11;
        private const int PlatinumLevel = 12;
        private const int ChampionLevel = 14;
        private const int DiamondLevel = 16;
        private const int HeroLevel = 17;
        private const int MythicLevel = 19;
        private const int LegendLevel = 20;
        private readonly IPlayerRepository _playerRepository;
        private readonly LevelTable _levelTable;
        private static readonly int[] RequiredLevels =
        {
            FreeLevel,
            FreeLevel,
            FreeLevel,
            FreeLevel,
            BronzeLevel,
            SilverLevel,
            GoldLevel,
            PlatinumLevel,
            DiamondLevel,
            LegendLevel,
            RookieLevel,
            ApprenticeLevel,
            SkilledLevel,
            VeteranLevel,
            ExpertLevel,
            MasterLevel,
            ElderLevel,
            ChampionLevel,
            HeroLevel,
            MythicLevel
        };
        private static readonly List<UnlockableAvatar> Avatars = RequiredLevels
            .Select((level, index) => new UnlockableAvatar(FirstAvatarId + index, level))
            .ToList();

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
            Player player = FindPlayers(displayName).FirstOrDefault();

            return player == null ? AvatarService.DefaultAvatarId : player.AvatarId;
        }

        /// <summary>
        /// Gets the level of a player, so the room can show it next to the name.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The level of the player, or zero for a guest, who has no level.</returns>
        public int GetLevel(string displayName)
        {
            Player player = FindPlayers(displayName).FirstOrDefault();
            int level = GuestLevel;

            if (player != null)
            {
                level = CalculateLevel(player);
            }

            return level;
        }

        /// <summary>
        /// Gets the avatar, level and experience of a player, with the experience where the level starts and
        /// where the next one starts.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The profile; a profile with the default values if the player does not exist.</returns>
        public PlayerProfile GetProfile(string displayName)
        {
            Player player = FindPlayers(displayName).FirstOrDefault();
            var profile = new PlayerProfile
            {
                AvatarId = AvatarService.DefaultAvatarId,
                Level = DefaultLevel,
                Experience = DefaultExperience,
                TitleId = AchievementCatalog.DefaultTitleId,
                FlagCode = string.Empty
            };

            if (player != null)
            {
                profile.AvatarId = player.AvatarId;
                profile.Level = CalculateLevel(player);
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
            Player player = FindPlayers(displayName).FirstOrDefault();
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

        /// <summary>
        /// Gets every avatar of the game, ordered by identifier.
        /// </summary>
        /// <returns>A copy of the list of avatars.</returns>
        public static List<UnlockableAvatar> GetAll()
        {
            return new List<UnlockableAvatar>(Avatars);
        }

        /// <summary>
        /// Looks for an avatar by its identifier.
        /// </summary>
        /// <param name="avatarId">Identifier to look for.</param>
        /// <param name="avatar">The avatar found; only valid when the method returns true.</param>
        /// <returns>True if the avatar exists.</returns>
        public static bool TryFind(int avatarId, out UnlockableAvatar avatar)
        {
            Func<UnlockableAvatar, bool> isSameId = candidate => candidate.Id == avatarId;
            avatar = Avatars.FirstOrDefault(isSameId);

            return avatar != null;
        }

        private int ReadTitleId(int playerId)
        {
            bool isNumber = int.TryParse(_playerRepository.FindTitle(playerId), out int titleId);

            return isNumber ? titleId : AchievementCatalog.DefaultTitleId;
        }

        private int CalculateLevel(Player player) => _levelTable.GetLevel(player.XP ?? DefaultExperience);

        private AvatarChangeStatus GetChangeStatus(Player player, int avatarId)
        {
            AvatarChangeStatus status = AvatarChangeStatus.Success;

            if (!AvatarService.TryFind(avatarId, out UnlockableAvatar avatar))
            {
                status = AvatarChangeStatus.AvatarNotFound;
            }
            else if (CalculateLevel(player) < avatar.RequiredLevel)
            {
                status = AvatarChangeStatus.AvatarLocked;
            }

            return status;
        }

        private List<Player> FindPlayers(string displayName)
        {
            var matches = new List<Player>();

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                matches = _playerRepository.FindByName(displayName);
            }

            return matches;
        }
    }
}
