using System;
using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// A single chat message exchanged inside a room, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class ChatMessage
    {
        /// <summary>
        /// Display name or username of the player who sent the message.
        /// </summary>
        [DataMember]
        public string Author { get; set; }

        /// <summary>
        /// Text of the message.
        /// </summary>
        [DataMember]
        public string Content { get; set; }

        /// <summary>
        /// Date and time at which the message was sent.
        /// </summary>
        [DataMember]
        public DateTime Timestamp { get; set; }
    }
}

