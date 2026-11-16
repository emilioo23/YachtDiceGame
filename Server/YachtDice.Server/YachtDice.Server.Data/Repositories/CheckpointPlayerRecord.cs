using System.Collections.Generic;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// A player of a game in progress with the boxes that the player already filled.
    /// </summary>
    public class CheckpointPlayerRecord
    {
        /// <summary>
        /// Gets or sets the name of the account of the player.
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player already lost for not coming back in time.
        /// </summary>
        public bool HasForfeited { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player already has the bonus of the upper section.
        /// </summary>
        public bool HasUpperBonus { get; set; }

        /// <summary>
        /// Gets or sets how many extra yachts the player made.
        /// </summary>
        public int YachtBonusCount { get; set; }

        /// <summary>
        /// Gets the boxes that the player already filled.
        /// </summary>
        public List<ScoreRecord> Scores { get; set; } = new List<ScoreRecord>();
    }
}
