using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// Sorts the players of a finished game by their points and gives each one a place. Players with the same
    /// points share the place, and the next place is skipped (1, 1, 3).
    /// </summary>
    public static class StandingsBuilder
    {
        /// <summary>
        /// Place of the winner.
        /// </summary>
        public const int FirstRank = 1;

        /// <summary>
        /// Creates the final scoreboard.
        /// </summary>
        /// <param name="entries">Players of the game, in any order.</param>
        /// <returns>The players from the first place to the last. Players that tie keep their order.</returns>
        public static IReadOnlyList<RankedStanding> Build(IEnumerable<StandingEntry> entries)
        {
            var sorted = entries.OrderByDescending(entry => entry.Total).ToList();
            var standings = new List<RankedStanding>();

            for (var position = 0; position < sorted.Count; position++)
            {
                standings.Add(new RankedStanding(FindRank(sorted, position), sorted[position]));
            }

            return standings;
        }

        private static int FindRank(IReadOnlyList<StandingEntry> sorted, int position)
        {
            var firstWithSameTotal = 0;

            while (sorted[firstWithSameTotal].Total != sorted[position].Total)
            {
                firstWithSameTotal++;
            }

            return firstWithSameTotal + FirstRank;
        }
    }
}
