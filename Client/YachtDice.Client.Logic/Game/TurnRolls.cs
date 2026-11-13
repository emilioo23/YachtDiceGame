namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// The rolls that the player in turn has left and the rolls that a turn gives.
    /// </summary>
    public sealed class TurnRolls
    {
        /// <summary>
        /// Initializes the rolls of a turn.
        /// </summary>
        /// <param name="rollsLeft">Rolls that the current player has left.</param>
        /// <param name="maxRolls">Rolls that a turn gives.</param>
        public TurnRolls(int rollsLeft, int maxRolls)
        {
            RollsLeft = rollsLeft;
            MaxRolls = maxRolls;
        }

        /// <summary>
        /// Gets the rolls that the current player has left.
        /// </summary>
        public int RollsLeft { get; }

        /// <summary>
        /// Gets the rolls that a turn gives.
        /// </summary>
        public int MaxRolls { get; }
    }
}
