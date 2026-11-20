using System.Collections.Generic;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Scorecard of one player in a past game, as the history shows it.
    /// </summary>
    public class SavedScorecard
    {
        /// <summary>
        /// Gets or sets the name of the player.
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Gets or sets the final points of the player.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player earned the bonus of the upper section.
        /// </summary>
        public bool HasUpperBonus { get; set; }

        /// <summary>
        /// Gets or sets how many extra Yachts the player scored.
        /// </summary>
        public int YachtBonusCount { get; set; }

        /// <summary>
        /// Gets or sets the points of each category, in the order of the scorecard.
        /// </summary>
        public List<SavedScore> Scores { get; set; } = new List<SavedScore>();
    }
}
