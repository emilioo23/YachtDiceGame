using System;
using System.Runtime.Serialization;

namespace YachtDiceGame.Network.Contracts
{
    [DataContract]
    public class ChatMessage
    {
        [DataMember]
        public string Author { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }
    }
}