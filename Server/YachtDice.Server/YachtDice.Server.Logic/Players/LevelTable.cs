using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// The experience that each level needs. The values come from the configuration of the server: the first
    /// one is the first level and must be zero, and each next level needs more experience than the one before.
    /// </summary>
    public sealed class LevelTable
    {
        private const int StartingExperience = 0;
        private const string NullThresholdsMessage = "The experience of each level cannot be null.";
        private const string EmptyThresholdsMessage = "There must be at least one level.";
        private const string InvalidFirstThresholdMessage = "The first level must need zero experience.";
        private const string UnorderedThresholdsMessage = "Each level must need more experience than the last one.";
        private const string NegativeExperienceMessage = "The experience of a player cannot be negative.";

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

            _thresholds = thresholds.ToList();
            ValidateThresholds(_thresholds);
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

        private static void ValidateThresholds(List<int> thresholds)
        {
            if (thresholds.Count == 0)
            {
                throw new ArgumentException(EmptyThresholdsMessage, nameof(thresholds));
            }

            if (thresholds[0] != StartingExperience)
            {
                throw new ArgumentException(InvalidFirstThresholdMessage, nameof(thresholds));
            }

            for (int levelIndex = 1; levelIndex < thresholds.Count; levelIndex++)
            {
                if (thresholds[levelIndex] <= thresholds[levelIndex - 1])
                {
                    throw new ArgumentException(UnorderedThresholdsMessage, nameof(thresholds));
                }
            }
        }
    }
}
