using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Information about the boxes of the scorecard that does not depend on a game.
    /// </summary>
    public static class ScoreCategories
    {
        private const int FaceToCategoryOffset = 1;

        /// <summary>
        /// Gets all the boxes in the order of the scorecard.
        /// </summary>
        public static IReadOnlyList<ScoreCategory> All { get; } =
            Enum.GetValues(typeof(ScoreCategory)).Cast<ScoreCategory>().ToList();

        /// <summary>
        /// Tells whether a box belongs to the upper section, the one that gives the bonus.
        /// </summary>
        /// <param name="category">Box to check.</param>
        /// <returns>True if the box is from Ones to Sixes.</returns>
        public static bool IsUpper(ScoreCategory category)
        {
            return category <= ScoreCategory.Sixes;
        }

        /// <summary>
        /// Gets the upper box that counts the dice with a value.
        /// </summary>
        /// <param name="faceValue">Value of the dice, from one to six.</param>
        /// <returns>The box that sums the dice with that value.</returns>
        public static ScoreCategory GetUpperCategory(int faceValue)
        {
            return (ScoreCategory)(faceValue - FaceToCategoryOffset);
        }

        /// <summary>
        /// Gets the value of the dice that an upper box counts.
        /// </summary>
        /// <param name="category">Upper box.</param>
        /// <returns>The value of the dice, from one to six.</returns>
        public static int GetFaceValue(ScoreCategory category)
        {
            return (int)category + FaceToCategoryOffset;
        }
    }
}
