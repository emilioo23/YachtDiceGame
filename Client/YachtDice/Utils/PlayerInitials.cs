using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the letter shown in the avatar of a player.
    /// </summary>
    public static class PlayerInitials
    {
        private const int FirstCharacterIndex = 0;
        private const int SingleCharacterLength = 1;

        /// <summary>
        /// Gets the initial of a name, in upper case.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>The first letter of the name, or the unknown initial if the name is blank.</returns>
        public static string GetInitial(string playerName)
        {
            string initial = Strings.Global_UnknownInitial;

            if (!string.IsNullOrWhiteSpace(playerName))
            {
                initial = playerName.Trim()
                    .Substring(FirstCharacterIndex, SingleCharacterLength)
                    .ToUpperInvariant();
            }

            return initial;
        }
    }
}
