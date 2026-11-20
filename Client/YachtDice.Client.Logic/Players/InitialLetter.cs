namespace YachtDice.Client.Logic.Players
{
    /// <summary>
    /// Chooses the letter that is shown as the picture of a player who has no avatar to show.
    /// </summary>
    public static class InitialLetter
    {
        private const int FirstCharacterIndex = 0;
        private const int SingleCharacterLength = 1;

        /// <summary>
        /// Gets the initial of a name, in upper case.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="unknownInitial">Text to use when the name is empty or has only spaces.</param>
        /// <returns>The first letter of the name, or <paramref name="unknownInitial"/> if the name is blank.</returns>
        public static string From(string playerName, string unknownInitial)
        {
            string initial = unknownInitial;

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
