namespace YachtDice.Client.Logic.Players
{
    /// <summary>
    /// Decides what the history of games shows about the rivals. Only players with an account are saved in
    /// the history, so a game against guests arrives without any name.
    /// </summary>
    public static class HistoryDisplay
    {
        private const string NameSeparator = ", ";

        /// <summary>
        /// Decides if a game has rivals that were saved with their name.
        /// </summary>
        /// <param name="opponents">Names of the rivals that the server sent, separated by commas.</param>
        /// <returns>True if there is at least one name.</returns>
        public static bool HasRegisteredOpponents(string opponents)
        {
            return !string.IsNullOrWhiteSpace(opponents);
        }

        /// <summary>
        /// Chooses the text that names the rivals of a game.
        /// </summary>
        /// <param name="opponents">Names of the rivals that the server sent, separated by commas.</param>
        /// <param name="guestText">Text to show when the rivals were guests and have no name.</param>
        /// <returns>The names of the rivals, or the text for guests when there are none.</returns>
        public static string ChooseOpponents(string opponents, string guestText)
        {
            return HasRegisteredOpponents(opponents) ? opponents : guestText;
        }

        /// <summary>
        /// Adds the computer to the rivals of a game that was played against it.
        /// </summary>
        /// <param name="opponents">Names of the rivals that the server sent, separated by commas.</param>
        /// <param name="computerText">Text that names the computer.</param>
        /// <returns>The names of the rivals followed by the computer, or only the computer if there are no names.</returns>
        public static string AddComputer(string opponents, string computerText)
        {
            return HasRegisteredOpponents(opponents) ? opponents + NameSeparator + computerText : computerText;
        }
    }
}
