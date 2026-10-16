using System.Runtime.Serialization;

namespace YachtDiceGame.Network.Contracts
{
    [DataContract]
    public class PlayerProfile
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int AvatarId { get; set; }
        
        [DataMember]
        public int Score { get; set; }
    }
}