using System;
using System.Collections.Generic;
using System.Globalization;

namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// Faces of the dice: how a row of dice is read, which resource shows each value and where the pips go.
    /// </summary>
    public static class DiceFaces
    {
        /// <summary>
        /// Number of the face that is shown when the image of another one is missing.
        /// </summary>
        public const int DefaultValue = 1;

        private const char Separator = ' ';
        private const string DimmedMarker = "-";
        private const int LowestValue = 1;
        private const int HighestValue = 6;
        private const int FirstCharacterIndex = 0;
        private const string KeyFormat = "Die_{0}";
        private const int NoValue = 0;
        private const int FaceOne = 1;
        private const int FaceTwo = 2;
        private const int FaceThree = 3;
        private const int FaceFour = 4;
        private const int FaceFive = 5;
        private const int FaceSix = 6;
        private const int Low = 0;
        private const int Middle = 1;
        private const int High = 2;
        private const string InvalidValueMessage = "The value of a die goes from zero to six.";
        private static readonly Dictionary<int, PipCell[]> Layouts = new Dictionary<int, PipCell[]>
        {
            { NoValue, new PipCell[] { } },
            { FaceOne, new[] { new PipCell(Middle, Middle) } },
            { FaceTwo, new[] { new PipCell(Low, Low), new PipCell(High, High) } },
            { FaceThree, new[] { new PipCell(Low, Low), new PipCell(Middle, Middle), new PipCell(High, High) } },
            {
                FaceFour,
                new[] { new PipCell(Low, Low), new PipCell(Low, High), new PipCell(High, Low), new PipCell(High, High) }
            },
            {
                FaceFive,
                new[]
                {
                    new PipCell(Low, Low),
                    new PipCell(Low, High),
                    new PipCell(Middle, Middle),
                    new PipCell(High, Low),
                    new PipCell(High, High)
                }
            },
            {
                FaceSix,
                new[]
                {
                    new PipCell(Low, Low),
                    new PipCell(Low, High),
                    new PipCell(Middle, Low),
                    new PipCell(Middle, High),
                    new PipCell(High, Low),
                    new PipCell(High, High)
                }
            }
        };

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

        /// <summary>
        /// Gets the resource name of the image of a face.
        /// </summary>
        /// <param name="value">Number of the face, from 1 to 6.</param>
        /// <returns>The resource name, for example Die_4.</returns>
        public static string Create(int value)
        {
            return string.Format(CultureInfo.InvariantCulture, KeyFormat, value);
        }

        /// <summary>
        /// Gets the cells that have a pip in a face.
        /// </summary>
        /// <param name="value">Value of the face from one to six; zero is a die that was not rolled yet.</param>
        /// <returns>The cells with a pip; none when the value is zero.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The value is not between zero and six.</exception>
        public static IReadOnlyList<PipCell> GetCells(int value)
        {
            PipCell[] cells;

            if (!Layouts.TryGetValue(value, out cells))
            {
                throw new ArgumentOutOfRangeException(nameof(value), InvalidValueMessage);
            }

            return cells;
        }

        private static void AddIfValid(List<DieDisplay> dice, string word)
        {
            bool isDimmed = word.EndsWith(DimmedMarker, StringComparison.Ordinal);
            string digits = word;

            if (isDimmed)
            {
                digits = word.Substring(FirstCharacterIndex, word.Length - DimmedMarker.Length);
            }

            bool isNumber = int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out int value);
            bool isFaceValue = value >= LowestValue && value <= HighestValue;

            if (isNumber && isFaceValue)
            {
                dice.Add(new DieDisplay { Value = value, IsDimmed = isDimmed });
            }
        }
    }
}
