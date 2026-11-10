namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Constants that describe the dice used in a game.
    /// </summary>
    public static class DiceRules
    {
        /// <summary>
        /// Number of dice that the players roll.
        /// </summary>
        public const int DiceCount = 5;

        /// <summary>
        /// Lowest value that a die can show.
        /// </summary>
        public const int MinimumFaceValue = 1;

        /// <summary>
        /// Highest value that a die can show.
        /// </summary>
        public const int MaximumFaceValue = 6;

        /// <summary>
        /// Number of rolls that a player has in each turn.
        /// </summary>
        public const int RollsPerTurn = 3;
    }
}
