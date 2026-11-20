using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Decides the moves that the server plays by itself: the box used when the time of a turn is over and
    /// the turn of a computer player. It takes the box with more points; when several boxes give the same
    /// points, it uses the one that is least valuable to keep, so a zero never burns a box like Yacht while
    /// an easy box is still open.
    /// </summary>
    public static class AutoPlayChooser
    {
        private const int NoPoints = 0;

        private static readonly List<ScoreCategory> FixedPointCategories = new List<ScoreCategory>
        {
            ScoreCategory.FullHouse,
            ScoreCategory.LittleStraight,
            ScoreCategory.BigStraight,
            ScoreCategory.Yacht
        };

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

        /// <summary>
        /// Chooses the dice that a computer player keeps before rolling again: the dice of the value that
        /// repeats the most, and the highest value when two values repeat the same number of times.
        /// </summary>
        /// <param name="dice">Values of the five dice.</param>
        /// <returns>One value for each die: true if the die keeps its value.</returns>
        public static bool[] ChooseHeldDice(IReadOnlyList<int> dice)
        {
            int keptValue = dice
                .GroupBy(value => value)
                .OrderByDescending(sameValues => sameValues.Count())
                .ThenByDescending(sameValues => sameValues.Key)
                .First()
                .Key;

            Func<int, bool> isKeptValue = value => value == keptValue;

            return dice.Select(isKeptValue).ToArray();
        }

        /// <summary>
        /// Tells whether a computer player stops rolling because the best box already gives its full points,
        /// like a straight or a Yacht, which another roll could only break.
        /// </summary>
        /// <param name="options">Boxes that the player can choose with the current dice.</param>
        /// <returns>True if the player should write the points now.</returns>
        public static bool IsWorthKeeping(IReadOnlyList<ScoreEntry> options)
        {
            ScoreCategory best = Choose(options);
            Func<ScoreEntry, bool> isBest = option => option.Category == best;

            return FixedPointCategories.Contains(best) && options.First(isBest).Points > NoPoints;
        }
    }
}
