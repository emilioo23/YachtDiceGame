using System;
using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Chat
{
    /// <summary>
    /// A single chat message exchanged inside a room, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class ChatMessage
    {
        /// <summary>
        /// Identifier that the server gave to the player who sent the message. The clients compare it with
        /// their own to know which messages are theirs.
        /// </summary>
        [DataMember]
        public string AuthorId { get; set; }

        /// <summary>
        /// Name that the screens show for the player who sent the message.
        /// </summary>
        [DataMember]
        public string Author { get; set; }

        /// <summary>
        /// Identifier of the avatar of the player who sent the message.
        /// </summary>
        [DataMember]
        public int AvatarId { get; set; }

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

