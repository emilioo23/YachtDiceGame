using System;
using System.Globalization;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Rules of the progression of a registered player. The winner of a game earns a base of experience points
    /// and the other players a smaller one, and every player adds one point for each point of the game. Every
    /// level needs more experience than the previous one: level 2 needs 200 points, level 3 needs 500, level 4
    /// needs 900 and so on.
    /// </summary>
    public static class PlayerProgression
    {
        /// <summary>
        /// Level that every new player has.
        /// </summary>
        public const int FirstLevel = 1;

        private const int WinnerBaseExperience = 100;
        private const int ParticipantBaseExperience = 30;
        private const int ThresholdFactor = 50;
        private const int ThresholdOffset = 2;
        private const string NegativeScoreFormat = "The final score cannot be negative, but it was {0}.";

        /// <summary>
        /// Calculates the experience that a player earns in a finished game.
        /// </summary>
        /// <param name="finalScore">Total points of the player when the game finished.</param>
        /// <param name="isWinner">True if the player won the game.</param>
        /// <returns>The experience points that the player earns.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The final score is negative.</exception>
        public static int CalculateExperience(int finalScore, bool isWinner)
        {
            if (finalScore < 0)
            {
                string message = string.Format(CultureInfo.InvariantCulture, NegativeScoreFormat, finalScore);

                throw new ArgumentOutOfRangeException(nameof(finalScore), message);
            }

            int baseExperience = isWinner ? WinnerBaseExperience : ParticipantBaseExperience;

            return baseExperience + finalScore;
        }

        /// <summary>
        /// Gets the highest level that a player reaches with an amount of experience.
        /// </summary>
        /// <param name="experience">Total experience points of the player.</param>
        /// <returns>The level of the player, never lower than the first level.</returns>
        public static int GetLevel(int experience)
        {
            int level = FirstLevel;

            while (experience >= GetRequiredExperience(level + 1))
            {
                level++;
            }

            return level;
        }

        /// <summary>
        /// Gets the experience that a player needs to reach a level.
        /// </summary>
        /// <param name="level">Level to reach.</param>
        /// <returns>The total experience points that the level needs; zero for the first level.</returns>
        public static long GetRequiredExperience(int level)
        {
            long levelsToClimb = level - FirstLevel;

            return ThresholdFactor * levelsToClimb * (level + ThresholdOffset);
        }
    }
}
