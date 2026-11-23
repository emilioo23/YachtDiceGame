using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Experience that each level needs and the experience that a player gains in a game.
    /// </summary>
    public sealed class LevelTable
    {
        /// <summary>
        /// Base experience of a player who wins the game.
        /// </summary>
        public const int WinnerBaseExperience = 100;

        /// <summary>
        /// Base experience of a player who completes the game without winning it.
        /// </summary>
        public const int LoserBaseExperience = 30;

        private const int StartingExperience = 0;
        private const int FirstLevel = 1;
        private const string NullThresholdsMessage = "The experience of each level cannot be null.";
        private const string EmptyThresholdsMessage = "There must be at least one level.";
        private const string InvalidFirstThresholdMessage = "The first level must need zero experience.";
        private const string UnorderedThresholdsMessage = "Each level must need more experience than the last one.";
        private const string NegativeExperienceMessage = "The experience of a player cannot be negative.";
        private const string InvalidLevelMessage = "The level must be at least one.";
        private const int NoPoints = 0;
        private const string NegativeTotalMessage = "The total of the scorecard cannot be negative.";
        private readonly List<int> _thresholds;

        /// <summary>
        /// Initializes the table with the experience that each level needs, from the first level to the last.
        /// </summary>
        /// <param name="thresholds">Experience needed to reach each level.</param>
        /// <exception cref="ArgumentNullException">The thresholds are null.</exception>
        /// <exception cref="ArgumentException">The thresholds are empty, do not start at zero or are not
        /// ascending.</exception>
        public LevelTable(IEnumerable<int> thresholds)
        {
            if (thresholds == null)
            {
                throw new ArgumentNullException(nameof(thresholds), NullThresholdsMessage);
            }

            List<int> levelThresholds = thresholds.ToList();

            if (levelThresholds.Count == 0)
            {
                throw new ArgumentException(EmptyThresholdsMessage, nameof(thresholds));
            }

            if (levelThresholds.First() != StartingExperience)
            {
                throw new ArgumentException(InvalidFirstThresholdMessage, nameof(thresholds));
            }

            if (!IsAscending(levelThresholds))
            {
                throw new ArgumentException(UnorderedThresholdsMessage, nameof(thresholds));
            }

            _thresholds = levelThresholds;
        }

        /// <summary>
        /// Gets the highest level that the table has.
        /// </summary>
        public int MaxLevel => _thresholds.Count;

        /// <summary>
        /// Gets the level that a player has with the given experience.
        /// </summary>
        /// <param name="experience">Accumulated experience of the player.</param>
        /// <returns>The level of the player, starting at one.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The experience is negative.</exception>
        public int GetLevel(int experience)
        {
            if (experience < StartingExperience)
            {
                throw new ArgumentOutOfRangeException(nameof(experience), NegativeExperienceMessage);
            }

            Func<int, bool> isReached = threshold => threshold <= experience;

            return _thresholds.Count(isReached);
        }

        /// <summary>
        /// Gets the experience that a level needs. A level above the last one counts as the last one.
        /// </summary>
        /// <param name="level">Level of the player, starting at one.</param>
        /// <returns>The experience where the level starts.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The level is lower than one.</exception>
        public int GetLevelStart(int level)
        {
            if (level < FirstLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(level), InvalidLevelMessage);
            }

            int levelIndex = Math.Min(level, MaxLevel) - 1;

            return _thresholds[levelIndex];
        }

        /// <summary>
        /// Gets the experience that the level after the given one needs. At the last level there is no next
        /// level, so it returns the experience of the last level.
        /// </summary>
        /// <param name="level">Level of the player, starting at one.</param>
        /// <returns>The experience where the next level starts.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The level is lower than one.</exception>
        public int GetNextLevelStart(int level)
        {
            if (level < FirstLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(level), InvalidLevelMessage);
            }

            return GetLevelStart(Math.Min(level + 1, MaxLevel));
        }

        /// <summary>
        /// Calculates the experience that a player earns for completing a game.
        /// </summary>
        /// <param name="total">Total of the scorecard of the player when the game finished.</param>
        /// <param name="isWinner">True if the player won the game.</param>
        /// <returns>The experience earned in the game.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The total is negative.</exception>
        public static int CalculateGain(int total, bool isWinner)
        {
            if (total < NoPoints)
            {
                throw new ArgumentOutOfRangeException(nameof(total), NegativeTotalMessage);
            }

            int baseExperience = isWinner ? WinnerBaseExperience : LoserBaseExperience;

            return baseExperience + total;
        }

        private static bool IsAscending(List<int> thresholds)
        {
            bool isAscending = true;

            for (int levelIndex = 1; levelIndex < thresholds.Count; levelIndex++)
            {
                isAscending = isAscending && thresholds[levelIndex] > thresholds[levelIndex - 1];
            }

            return isAscending;
        }
    }
}
