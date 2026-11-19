using System;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// The places of the scene where the dice lie: the row in the middle of the table where they wait before
    /// the first roll, the row under the table for the dice that the player keeps, and the disc of the table
    /// where the dice land.
    /// </summary>
    public static class DiceTableLayout
    {
        /// <summary>
        /// Distance from the center of the table to the farthest place where the center of a die can land.
        /// </summary>
        public const double LandingRadius = 2.3;

        /// <summary>
        /// Distance toward the player, from the center of the table, of the row of the dice that are kept.
        /// </summary>
        public const double RackDepth = 5.9;

        /// <summary>
        /// Distance between the centers of two dice that are next to each other in a row.
        /// </summary>
        public const double RowSpacing = 1.7;

        private const double Half = 0.5;
        private const double TableCenter = 0;
        private const string InvalidDieMessage = "The position of the die is not one of the five dice.";
        private const string InvalidRackMessage = "The position of the die must be inside the dice of the row.";

        /// <summary>
        /// Gets the place of a die in the row that crosses the middle of the table.
        /// </summary>
        /// <param name="index">Position of the die, from zero to four.</param>
        /// <returns>The place where the die waits.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The position is not from zero to four.</exception>
        public static DiceSlot GetRowSlot(int index)
        {
            if (index < 0 || index >= HeldDiceSelection.DiceCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), InvalidDieMessage);
            }

            return new DiceSlot((index - ((HeldDiceSelection.DiceCount - 1) * Half)) * RowSpacing, TableCenter);
        }

        /// <summary>
        /// Gets the place of a die in the row under the table. The row is centered, so it grows to both sides.
        /// </summary>
        /// <param name="position">Position of the die among the dice that are kept, from zero.</param>
        /// <param name="count">How many dice are kept.</param>
        /// <returns>The place where the die waits.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The position is not inside the dice that are kept.</exception>
        public static DiceSlot GetRackSlot(int position, int count)
        {
            if (count < 1 || position < 0 || position >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(position), InvalidRackMessage);
            }

            return new DiceSlot((position - ((count - 1) * Half)) * RowSpacing, RackDepth);
        }
    }
}
