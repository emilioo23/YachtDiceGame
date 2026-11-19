namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// A place of the table where a die lies, measured from the center of the table.
    /// </summary>
    public sealed class DiceSlot
    {
        /// <summary>
        /// Initializes the place.
        /// </summary>
        /// <param name="x">Position to the right of the center of the table.</param>
        /// <param name="z">Position toward the player, from the center of the table.</param>
        public DiceSlot(double x, double z)
        {
            X = x;
            Z = z;
        }

        /// <summary>
        /// Gets the position to the right of the center of the table.
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Gets the position toward the player, from the center of the table.
        /// </summary>
        public double Z { get; }
    }
}
