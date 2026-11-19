namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// The two places where a die can be: lying on the table or in the row under the table, where the dice that
    /// the player keeps for the next roll wait.
    /// </summary>
    public enum DiceLocation
    {
        /// <summary>
        /// The die lies on the table and is thrown in the next roll.
        /// </summary>
        Table,

        /// <summary>
        /// The die is in the row under the table because the player keeps it.
        /// </summary>
        Rack
    }
}
