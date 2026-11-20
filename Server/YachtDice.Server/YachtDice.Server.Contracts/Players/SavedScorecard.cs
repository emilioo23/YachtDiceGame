using System.Collections.Generic;
using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Players
{
    /// <summary>
    /// Scorecard of one player in a past game.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class SavedScorecard
    {
        /// <summary>
        /// Name of the player.
        /// </summary>
        [DataMember]
        public string PlayerName { get; set; }

        /// <summary>
        /// Final points of the player.
        /// </summary>
        [DataMember]
        public int Total { get; set; }

        /// <summary>
        /// Whether the player earned the bonus of the upper section.
        /// </summary>
        [DataMember]
        public bool HasUpperBonus { get; set; }

        /// <summary>
        /// How many extra Yachts the player scored.
        /// </summary>
        [DataMember]
        public int YachtBonusCount { get; set; }

        /// <summary>
        /// Points of each category, in the order of the scorecard.
        /// </summary>
        [DataMember]
        public List<SavedScore> Scores { get; set; }
    }
}
