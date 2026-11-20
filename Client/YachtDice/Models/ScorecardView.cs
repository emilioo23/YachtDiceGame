using System.Collections.Generic;

namespace YachtDice.Models
{
    /// <summary>
    /// Scorecard of one player of a past game, as the window of the scorecards shows it.
    /// </summary>
    public class ScorecardView
    {
        /// <summary>
        /// Gets or sets the title with the name of the player and the final points.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets one line for each category and for each bonus.
        /// </summary>
        public List<string> Lines { get; set; }
    }
}
