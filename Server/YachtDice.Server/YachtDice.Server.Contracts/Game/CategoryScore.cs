using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Game
{
    /// <summary>
    /// A box of the scorecard of a player.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class CategoryScore
    {
        /// <summary>
        /// The box.
        /// </summary>
        [DataMember]
        public ScoreCategory Category { get; set; }

        /// <summary>
        /// Whether the box already has points.
        /// </summary>
        [DataMember]
        public bool IsFilled { get; set; }

        /// <summary>
        /// Points of the box. They are zero while the box is open.
        /// </summary>
        [DataMember]
        public int Points { get; set; }
    }
}
