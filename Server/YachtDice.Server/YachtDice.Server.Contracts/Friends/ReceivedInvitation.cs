using System;
using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Friends
{
    /// <summary>
    /// An invitation to a room that a player received.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class ReceivedInvitation
    {
        /// <summary>
        /// Name of the friend who sent it.
        /// </summary>
        [DataMember]
        public string SenderName { get; set; }

        /// <summary>
        /// Code of the room. Only the invited player receives it.
        /// </summary>
        [DataMember]
        public string RoomCode { get; set; }

        /// <summary>
        /// Moment when it was sent, in universal time.
        /// </summary>
        [DataMember]
        public DateTime SentAt { get; set; }

        /// <summary>
        /// Whether the room still exists, so the player can try to join it.
        /// </summary>
        [DataMember]
        public bool IsRoomOpen { get; set; }
    }
}
