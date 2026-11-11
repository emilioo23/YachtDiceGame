using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Counts how many dice show each value, so that the rules can ask questions about a roll.
    /// </summary>
    internal sealed class DiceTally
    {
        private const int NoValue = 0;
        private const int SingleOccurrence = 1;
        private const string InvalidCountMessage = "A roll must have exactly five dice.";
        private const string InvalidValueMessage = "A die can only show a value from one to six.";

        private readonly int[] _countsByValue = new int[DiceRules.MaximumFaceValue + DiceRules.MinimumFaceValue];

        /// <summary>
        /// Initializes the tally of a roll.
        /// </summary>
        /// <param name="dice">Values shown by the dice.</param>
        public DiceTally(IReadOnlyList<int> dice)
        {
            Validate(dice);

            foreach (var value in dice)
            {
                _countsByValue[value]++;
                Sum += value;
            }
        }

        /// <summary>
        /// Gets the sum of all the dice.
        /// </summary>
        public int Sum { get; }

        /// <summary>
        /// Gets the biggest number of dice that show the same value.
        /// </summary>
        public int HighestCount => _countsByValue.Max();

        /// <summary>
        /// Gets a value indicating whether the five dice show the same value.
        /// </summary>
        public bool IsYacht => HighestCount == DiceRules.DiceCount;

        /// <summary>
        /// Gets how many dice show a value.
        /// </summary>
        /// <param name="value">Value of the dice.</param>
        /// <returns>Number of dice with that value.</returns>
        public int CountOf(int value)
        {
            return _countsByValue[value];
        }

        /// <summary>
        /// Gets the value that appears at least a number of times.
        /// </summary>
        /// <param name="minimumCount">Least number of dice with the same value.</param>
        /// <returns>The highest value that appears enough times, or zero if there is none.</returns>
        public int GetValueRepeatedAtLeast(int minimumCount)
        {
            Func<int, bool> hasMinimumCount = value => _countsByValue[value] >= minimumCount;

            return Enumerable.Range(DiceRules.MinimumFaceValue, DiceRules.MaximumFaceValue)
                .Where(hasMinimumCount)
                .DefaultIfEmpty(NoValue)
                .Max();
        }

        /// <summary>
        /// Tells whether some value appears an exact number of times.
        /// </summary>
        /// <param name="count">Number of dice with the same value.</param>
        /// <returns>True if at least one value appears exactly that number of times.</returns>
        public bool HasValueExactly(int count)
        {
            Func<int, bool> isSameCount = valueCount => valueCount == count;

            return _countsByValue.Any(isSameCount);
        }

        /// <summary>
        /// Tells whether the dice show every value of a range once.
        /// </summary>
        /// <param name="firstValue">Lowest value of the range.</param>
        /// <param name="lastValue">Highest value of the range.</param>
        /// <returns>True if each value from the first to the last appears exactly once.</returns>
        public bool ShowsEachValueOnce(int firstValue, int lastValue)
        {
            Func<int, bool> isSingle = value => _countsByValue[value] == SingleOccurrence;

            return Enumerable.Range(firstValue, lastValue - firstValue + 1)
                .All(isSingle);
        }

        private static void Validate(IReadOnlyList<int> dice)
        {
            if (dice.Count != DiceRules.DiceCount)
            {
                throw new ArgumentException(InvalidCountMessage, nameof(dice));
            }

            if (dice.Any(IsOutOfRange))
            {
                throw new ArgumentException(InvalidValueMessage, nameof(dice));
            }
        }

        private static bool IsOutOfRange(int value)
        {
            return value < DiceRules.MinimumFaceValue || value > DiceRules.MaximumFaceValue;
        }
    }
}
