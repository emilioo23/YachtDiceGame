using System;

namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// The dice that the player decided to keep before the next roll.
    /// </summary>
    public sealed class HeldDiceSelection
    {
        private const int DiceCount = 5;

        private readonly bool[] _held = new bool[DiceCount];

        /// <summary>
        /// Tells whether a die is held.
        /// </summary>
        /// <param name="index">Position of the die, from zero to four.</param>
        /// <returns>True if the die is held.</returns>
        public bool IsHeld(int index)
        {
            return _held[index];
        }

        /// <summary>
        /// Holds a die that is free, or frees a die that is held.
        /// </summary>
        /// <param name="index">Position of the die, from zero to four.</param>
        /// <param name="canHold">False when holding is not allowed, for example before the first roll.</param>
        /// <returns>True if the selection changed.</returns>
        public bool Toggle(int index, bool canHold)
        {
            if (canHold)
            {
                _held[index] = !_held[index];
            }

            return canHold;
        }

        /// <summary>
        /// Frees all the dice, for example when the turn changes.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_held, 0, _held.Length);
        }

        /// <summary>
        /// Gets a copy of the selection, with one value for each die, as the server expects it.
        /// </summary>
        /// <returns>True for each die that is held.</returns>
        public bool[] ToArray()
        {
            return (bool[])_held.Clone();
        }
    }
}
