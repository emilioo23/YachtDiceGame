using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Game
{
    /// <summary>
    /// A box that the current player can choose with the current dice.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class ScoreChoice
    {
        /// <summary>
        /// The box that can be chosen.
        /// </summary>
        [DataMember]
        public ScoreCategory Category { get; set; }

        /// <summary>
        /// Points that the box would receive.
        /// </summary>
        [DataMember]
        public int Points { get; set; }
    }
}
