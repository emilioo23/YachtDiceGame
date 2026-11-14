using System;

namespace YachtDice.Client.Logic.Players
{
    /// <summary>
    /// How full the experience bar of a level is: zero when the player just reached the level and one when
    /// the player is about to reach the next one. At the last level the bar is always full.
    /// </summary>
    public static class LevelProgress
    {
        /// <summary>
        /// Fraction of a bar that is empty.
        /// </summary>
        public const double EmptyFraction = 0;

        /// <summary>
        /// Fraction of a bar that is full.
        /// </summary>
        public const double FullFraction = 1;

        /// <summary>
        /// Calculates how full the bar of the current level is.
        /// </summary>
        /// <param name="experience">Accumulated experience of the player.</param>
        /// <param name="levelStart">Experience where the current level starts.</param>
        /// <param name="nextLevelStart">Experience where the next level starts.</param>
        /// <returns>A value from <see cref="EmptyFraction"/> to <see cref="FullFraction"/>.</returns>
        public static double GetFraction(int experience, int levelStart, int nextLevelStart)
        {
            double fraction = FullFraction;
            bool hasNextLevel = nextLevelStart > levelStart;

            if (hasNextLevel)
            {
                double gainedInLevel = experience - levelStart;
                double neededInLevel = nextLevelStart - levelStart;

                fraction = Math.Max(EmptyFraction, Math.Min(FullFraction, gainedInLevel / neededInLevel));
            }

            return fraction;
        }

        /// <summary>
        /// Tells whether the experience earned in a game made the player reach the current level.
        /// </summary>
        /// <param name="experience">Accumulated experience of the player after the game.</param>
        /// <param name="experienceGained">Experience earned in the game.</param>
        /// <param name="levelStart">Experience where the current level starts.</param>
        /// <returns>True if the player had less experience than the level needs before the game.</returns>
        public static bool IsNewLevel(int experience, int experienceGained, int levelStart)
        {
            int experienceBefore = experience - experienceGained;

            return experienceBefore < levelStart;
        }
    }
}
