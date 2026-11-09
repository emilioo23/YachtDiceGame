using System;
using System.Collections.Generic;
using System.Globalization;

namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// Reads the short text that describes the dice of an example, for instance "3 3 3 5- 6-". Each word is the
    /// number of a die, and a minus sign after it means that the die does not count for the points.
    /// </summary>
    public static class DiceRowParser
    {
        private const char Separator = ' ';
        private const string DimmedMarker = "-";
        private const int LowestValue = 1;
        private const int HighestValue = 6;

        /// <summary>
        /// Converts the text into the dice that must be drawn. Words that are not a number from 1 to 6 are
        /// ignored, so a typing mistake in an example does not break the window.
        /// </summary>
        /// <param name="text">Text with the dice separated by spaces; it can be empty.</param>
        /// <returns>The dice, in the same order as in the text; an empty list if there are none.</returns>
        public static List<DieDisplay> Parse(string text)
        {
            var dice = new List<DieDisplay>();

            if (!string.IsNullOrWhiteSpace(text))
            {
                foreach (string word in text.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries))
                {
                    AddIfValid(dice, word);
                }
            }

            return dice;
        }

        private static void AddIfValid(List<DieDisplay> dice, string word)
        {
            bool isDimmed = word.EndsWith(DimmedMarker, StringComparison.Ordinal);
            string digits = word;

            if (isDimmed)
            {
                digits = word.Substring(0, word.Length - DimmedMarker.Length);
            }

            if (int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out int value)
                && value >= LowestValue
                && value <= HighestValue)
            {
                dice.Add(new DieDisplay { Value = value, IsDimmed = isDimmed });
            }
        }
    }
}
