using YachtDice.Client.Logic.Players;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the letter shown in the avatar of a player.
    /// </summary>
    public static class PlayerInitials
    {
        /// <summary>
        /// Gets the initial of a name, in upper case.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>The first letter of the name, or the unknown initial if the name is blank.</returns>
        public static string GetInitial(string playerName)
        {
            return InitialLetter.From(playerName, Strings.Global_UnknownInitial);
        }
    }
}
