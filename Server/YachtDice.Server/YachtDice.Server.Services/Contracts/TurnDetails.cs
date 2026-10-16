using System.Runtime.Serialization;

namespace YachtDiceGame.Network.Contracts
{
    [DataContract]
    public class TurnDetails
    {
        [DataMember]
        public string CurrentTurnUsername { get; set; }

        [DataMember]
        public int RollsLeft { get; set; }
    }
}