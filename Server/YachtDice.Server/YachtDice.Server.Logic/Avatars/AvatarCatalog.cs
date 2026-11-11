using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Logic.Avatars
{
    /// <summary>
    /// List of the avatars of the game and the level that each one needs. The first ones are free for every
    /// player and the others are unlocked as the player levels up. The identifier of each avatar is its
    /// position in the list of levels, counting from the first identifier.
    /// </summary>
    public static class AvatarCatalog
    {
        /// <summary>
        /// Identifier of the avatar that every new player has.
        /// </summary>
        public const int DefaultAvatarId = FirstAvatarId;

        private const int FirstAvatarId = 1;
        private const int FreeLevel = 1;
        private const int BronzeLevel = 3;
        private const int SilverLevel = 5;
        private const int GoldLevel = 8;
        private const int PlatinumLevel = 12;
        private const int DiamondLevel = 16;
        private const int LegendLevel = 20;

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
            LegendLevel
        };

        private static readonly List<UnlockableAvatar> Avatars = CreateAvatars();

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

        /// <summary>
        /// Creates one avatar for each required level, with consecutive identifiers.
        /// </summary>
        /// <returns>The avatars of the game ordered by identifier.</returns>
        private static List<UnlockableAvatar> CreateAvatars()
        {

            return RequiredLevels.Select((level, index) => new UnlockableAvatar(FirstAvatarId + index, level)).ToList();
        }
    }
}
