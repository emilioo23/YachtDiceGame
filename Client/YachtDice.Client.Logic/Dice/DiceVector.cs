using System;

namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// A point or a direction in the three-dimensional space of the table. The table is the plane of X and Z,
    /// and Y goes up.
    /// </summary>
    public sealed class DiceVector
    {
        /// <summary>
        /// Initializes the vector.
        /// </summary>
        /// <param name="x">Position to the right of the center of the table.</param>
        /// <param name="y">Height above the table.</param>
        /// <param name="z">Position toward the player, from the center of the table.</param>
        public DiceVector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets the position to the right of the center of the table.
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Gets the height above the table.
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// Gets the position toward the player, from the center of the table.
        /// </summary>
        public double Z { get; }

        /// <summary>
        /// Creates the vector that points to the opposite direction.
        /// </summary>
        /// <returns>The vector with the three values changed of sign.</returns>
        public DiceVector Negate()
        {
            return new DiceVector(-X, -Y, -Z);
        }

        /// <summary>
        /// Creates a vector with its values rounded, so the small errors of the calculation can be ignored.
        /// </summary>
        /// <param name="decimals">Decimal places that are kept.</param>
        /// <returns>The rounded vector.</returns>
        public DiceVector Round(int decimals)
        {
            return new DiceVector(Math.Round(X, decimals), Math.Round(Y, decimals), Math.Round(Z, decimals));
        }
    }
}
