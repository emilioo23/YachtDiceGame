using System;
using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Rooms
{
    /// <summary>
    /// An expulsion that a player sees in the history: who expelled whom, in which room and when.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class KickHistoryEntry
    {
        /// <summary>
        /// Name of the host who expelled the player.
        /// </summary>
        [DataMember]
        public string HostName { get; set; }

        /// <summary>
        /// Name of the player who was expelled.
        /// </summary>
        [DataMember]
        public string TargetName { get; set; }

        /// <summary>
        /// Code of the room where it happened.
        /// </summary>
        [DataMember]
        public string RoomCode { get; set; }

        /// <summary>
        /// Moment of the expulsion, in universal time.
        /// </summary>
        [DataMember]
        public DateTime Date { get; set; }
    }
}
