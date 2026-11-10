using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Client.Logic.Avatars
{
    /// <summary>
    /// Prepares the list of avatars that the profile window shows, marking which ones the player can use.
    /// </summary>
    public static class AvatarOptionBuilder
    {
        /// <summary>
        /// Builds one option for each avatar of the catalog, ordered by identifier.
        /// </summary>
        /// <param name="avatars">Avatars of the catalog with the level that each one needs.</param>
        /// <param name="playerLevel">Level of the player.</param>
        /// <param name="selectedAvatarId">Identifier of the avatar that the player has chosen.</param>
        /// <returns>The options; an empty list if the catalog is empty.</returns>
        public static List<AvatarOption> Build(
            IEnumerable<AvatarRequirement> avatars,
            int playerLevel,
            int selectedAvatarId)
        {
            List<AvatarOption> options = avatars
                .OrderBy(avatar => avatar.Id)
                .Select(avatar => new AvatarOption
                {
                    Id = avatar.Id,
                    RequiredLevel = avatar.RequiredLevel,
                    IsUnlocked = playerLevel >= avatar.RequiredLevel,
                    IsSelected = avatar.Id == selectedAvatarId
                })
                .ToList();

            return options;
        }
    }
}
