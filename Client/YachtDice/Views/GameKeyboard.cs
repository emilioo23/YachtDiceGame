using System.Collections.Generic;
using System.Windows.Input;

namespace YachtDice.Views
{
    /// <summary>
    /// Keys that play the game without the mouse: the number keys from 1 to 5, on the top row or on the numeric
    /// keypad, hold or release the die in that position.
    /// </summary>
    public static class GameKeyboard
    {
        private const int FirstDie = 0;
        private const int SecondDie = 1;
        private const int ThirdDie = 2;
        private const int FourthDie = 3;
        private const int FifthDie = 4;

        private static readonly Dictionary<Key, int> DieIndexes = new Dictionary<Key, int>
        {
            { Key.D1, FirstDie },
            { Key.D2, SecondDie },
            { Key.D3, ThirdDie },
            { Key.D4, FourthDie },
            { Key.D5, FifthDie },
            { Key.NumPad1, FirstDie },
            { Key.NumPad2, SecondDie },
            { Key.NumPad3, ThirdDie },
            { Key.NumPad4, FourthDie },
            { Key.NumPad5, FifthDie }
        };

        /// <summary>
        /// Finds the die that a key holds or releases.
        /// </summary>
        /// <param name="key">Key that the player pressed.</param>
        /// <param name="dieIndex">Position of the die, from 0 to 4, when the key is a die key.</param>
        /// <returns>True if the key holds or releases a die.</returns>
        public static bool TryGetDieIndex(Key key, out int dieIndex)
        {
            return DieIndexes.TryGetValue(key, out dieIndex);
        }
    }
}
