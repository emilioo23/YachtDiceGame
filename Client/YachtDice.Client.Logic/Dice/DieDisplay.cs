namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// A die as the "How to play" window shows it in an example.
    /// </summary>
    public sealed class DieDisplay
    {
        /// <summary>
        /// Gets or sets the number that the die shows, from 1 to 6.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the die is shown faded because it does not count for the
        /// points of the example.
        /// </summary>
        public bool IsDimmed { get; set; }
    }
}
