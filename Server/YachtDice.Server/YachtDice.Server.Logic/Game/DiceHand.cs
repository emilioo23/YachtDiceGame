using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// The five dice of a roll. It knows how many dice show each face, so the rules of the scorecard can ask
    /// questions about the roll.
    /// </summary>
    public sealed class DiceHand
    {
        /// <summary>
        /// Number of dice that the players roll.
        /// </summary>
        public const int DiceCount = 5;

        /// <summary>
        /// Lowest value that a die can show.
        /// </summary>
        public const int MinimumFaceValue = 1;

        /// <summary>
        /// Highest value that a die can show.
        /// </summary>
        public const int MaximumFaceValue = 6;

        private const int NoValue = 0;
        private const int SingleOccurrence = 1;
        private const int FaceCount = MaximumFaceValue - MinimumFaceValue + 1;
        private const int CountsLength = MaximumFaceValue + 1;
        private const string NullDiceMessage = "The dice of a roll cannot be null.";
        private const string InvalidCountMessage = "A roll must have exactly five dice.";
        private const string InvalidValueMessage = "A die can only show a value from one to six.";

        private readonly int[] _countsByFace = new int[CountsLength];

        /// <summary>
        /// Initializes the hand with the values shown by the dice.
        /// </summary>
        /// <param name="dice">Values shown by the five dice.</param>
        /// <exception cref="ArgumentNullException">The dice are null.</exception>
        /// <exception cref="ArgumentException">There are not exactly five dice.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A die shows a value out of one to six.</exception>
        public DiceHand(IReadOnlyList<int> dice)
        {
            if (dice == null)
            {
                throw new ArgumentNullException(nameof(dice), NullDiceMessage);
            }

            if (dice.Count != DiceCount)
            {
                throw new ArgumentException(InvalidCountMessage, nameof(dice));
            }

            Func<int, bool> isOutOfRange = value => value < MinimumFaceValue || value > MaximumFaceValue;

            if (dice.Any(isOutOfRange))
            {
                throw new ArgumentOutOfRangeException(nameof(dice), InvalidValueMessage);
            }

            foreach (int value in dice)
            {
                _countsByFace[value]++;
            }

            Sum = dice.Sum();
        }

        /// <summary>
        /// Gets the sum of all the dice.
        /// </summary>
        public int Sum { get; }

        /// <summary>
        /// Gets a value indicating whether the five dice show the same value.
        /// </summary>
        public bool IsYacht => _countsByFace.Max() == DiceCount;

        /// <summary>
        /// Gets how many dice show a face.
        /// </summary>
        /// <param name="faceValue">Value of the face, from one to six.</param>
        /// <returns>Number of dice that show that face.</returns>
        public int CountFace(int faceValue)
        {
            return _countsByFace[faceValue];
        }

        /// <summary>
        /// Gets the highest face that at least a number of dice show.
        /// </summary>
        /// <param name="minimumCount">Least number of dice with the same face.</param>
        /// <returns>The highest face that appears enough times, or zero if there is none.</returns>
        public int GetFaceRepeatedAtLeast(int minimumCount)
        {
            Func<int, bool> hasMinimumCount = faceValue => _countsByFace[faceValue] >= minimumCount;

            return Enumerable.Range(MinimumFaceValue, FaceCount)
                .Where(hasMinimumCount)
                .DefaultIfEmpty(NoValue)
                .Max();
        }

        /// <summary>
        /// Tells whether some face appears an exact number of times.
        /// </summary>
        /// <param name="count">Number of dice with the same face.</param>
        /// <returns>True if at least one face appears exactly that number of times.</returns>
        public bool HasFaceExactly(int count)
        {
            Func<int, bool> isSameCount = faceCount => faceCount == count;

            return _countsByFace.Any(isSameCount);
        }

        /// <summary>
        /// Tells whether the dice show every face of a range once.
        /// </summary>
        /// <param name="firstFace">Lowest face of the range.</param>
        /// <param name="lastFace">Highest face of the range.</param>
        /// <returns>True if each face from the first to the last appears exactly once.</returns>
        public bool ShowsEachFaceOnce(int firstFace, int lastFace)
        {
            Func<int, bool> isSingle = faceValue => _countsByFace[faceValue] == SingleOccurrence;

            return Enumerable.Range(firstFace, lastFace - firstFace + 1).All(isSingle);
        }
    }
}
