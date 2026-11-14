using System.Collections.Generic;
using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Game
{
    /// <summary>
    /// A player of a game and the scorecard of the player.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class PlayerScorecard
    {
        /// <summary>
        /// Identifier of the player.
        /// </summary>
        [DataMember]
        public string PlayerId { get; set; }

        /// <summary>
        /// Name that the screens show for the player.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Whether the player is connected to the game.
        /// </summary>
        [DataMember]
        public bool IsPresent { get; set; }

        /// <summary>
        /// Points of the upper section.
        /// </summary>
        [DataMember]
        public int UpperSubtotal { get; set; }

        /// <summary>
        /// Bonus of the upper section.
        /// </summary>
        [DataMember]
        public int UpperBonus { get; set; }

        /// <summary>
        /// Bonus of the extra Yachts.
        /// </summary>
        [DataMember]
        public int YachtBonus { get; set; }

        /// <summary>
        /// Total of the scorecard, with the bonuses.
        /// </summary>
        [DataMember]
        public int Total { get; set; }

        /// <summary>
        /// Whether the player won the game. It is only set when the game is over.
        /// </summary>
        [DataMember]
        public bool IsWinner { get; set; }

        /// <summary>
        /// The experience that the player earned in the game. It is only set when the game is over, and it is
        /// zero for the players who left before the end.
        /// </summary>
        [DataMember]
        public int ExperienceGained { get; set; }

        /// <summary>
        /// The boxes of the scorecard, in the order of the scorecard.
        /// </summary>
        [DataMember]
        public List<CategoryScore> Scores { get; set; }
    }
}
