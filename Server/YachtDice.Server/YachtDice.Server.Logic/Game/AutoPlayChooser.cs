using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Decides which box is used when the time of a turn is over. It takes the box with more points; when
    /// several boxes give the same points, it uses the one that is least valuable to keep, so a zero never
    /// burns a box like Yacht while an easy box is still open.
    /// </summary>
    public static class AutoPlayChooser
    {
        private static readonly List<ScoreCategory> SacrificeOrder = new List<ScoreCategory>
        {
            ScoreCategory.Ones,
            ScoreCategory.Twos,
            ScoreCategory.Threes,
            ScoreCategory.Fours,
            ScoreCategory.Fives,
            ScoreCategory.Sixes,
            ScoreCategory.Choice,
            ScoreCategory.FourOfAKind,
            ScoreCategory.FullHouse,
            ScoreCategory.LittleStraight,
            ScoreCategory.BigStraight,
            ScoreCategory.Yacht
        };

        /// <summary>
        /// Chooses the box for a player who did not play in time.
        /// </summary>
        /// <param name="options">Boxes that the player can choose with the current dice.</param>
        /// <returns>The box to use.</returns>
        public static ScoreCategory Choose(IReadOnlyList<ScoreEntry> options)
        {
            return options
                .OrderByDescending(option => option.Points)
                .ThenBy(option => SacrificeOrder.IndexOf(option.Category))
                .First()
                .Category;
        }
    }
}
