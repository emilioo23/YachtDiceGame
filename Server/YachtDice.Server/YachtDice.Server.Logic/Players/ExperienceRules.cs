using System;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// The experience that a player earns for completing a game: the winners get more base experience than
    /// the rest, and everybody also gets one point of experience for each point of their scorecard.
    /// </summary>
    public static class ExperienceRules
    {
        /// <summary>
        /// Base experience of a player who wins the game.
        /// </summary>
        public const int WinnerBaseExperience = 100;

        /// <summary>
        /// Base experience of a player who completes the game without winning it.
        /// </summary>
        public const int LoserBaseExperience = 30;

        private const int NoPoints = 0;
        private const string NegativeTotalMessage = "The total of the scorecard cannot be negative.";

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
    }
}
