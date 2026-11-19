using System;

namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// The places of the table where the five dice lie. They are at the corners of a pentagon, far enough from
    /// each other so two dice never cross even if they land turned.
    /// </summary>
    public static class DiceTableLayout
    {
        /// <summary>
        /// Distance from the center of the table to the place of each die.
        /// </summary>
        public const double SlotRadius = 2.4;

        private const int SlotCount = 5;
        private const double FullTurnRadians = 2 * Math.PI;
        private const double FirstSlotAngle = -Math.PI / 2;
        private const string InvalidSlotMessage = "The table has five places for the dice.";

        /// <summary>
        /// Gets the place of a die.
        /// </summary>
        /// <param name="index">Position of the die, from zero to four.</param>
        /// <returns>The place where the die lies.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The position is not from zero to four.</exception>
        public static DiceSlot GetSlot(int index)
        {
            if (index < 0 || index >= SlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), InvalidSlotMessage);
            }

            double angle = FirstSlotAngle + (index * FullTurnRadians / SlotCount);

            return new DiceSlot(SlotRadius * Math.Cos(angle), SlotRadius * Math.Sin(angle));
        }
    }
}
