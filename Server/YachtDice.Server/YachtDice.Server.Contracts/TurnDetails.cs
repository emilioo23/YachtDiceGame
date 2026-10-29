using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Information about the current turn of a game, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class TurnDetails
    {
        /// <summary>
        /// Username of the player whose turn is active.
        /// </summary>
        [DataMember]
        public string CurrentTurnUsername { get; set; }

        /// <summary>
        /// Number of rolls still available in the current turn.
        /// </summary>
        [DataMember]
        public int RollsLeft { get; set; }
    }
}

