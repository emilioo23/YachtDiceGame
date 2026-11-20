namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// One die of the current turn of a game in progress.
    /// </summary>
    public class RolledDieRecord
    {
        /// <summary>
        /// Gets or sets the position of the die, from 1 to 5.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the value of the die, from 1 to 6.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player kept the die for the next roll.
        /// </summary>
        public bool IsHeld { get; set; }
    }
}
