using System.Collections.Generic;
using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Rooms
{
    /// <summary>
    /// Current state of a room, as seen by the clients.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class RoomState
    {
        /// <summary>
        /// Code that the players type to enter the room.
        /// </summary>
        [DataMember]
        public string Code { get; set; }

        /// <summary>
        /// Identifier of the player who controls the room.
        /// </summary>
        [DataMember]
        public string HostId { get; set; }

        /// <summary>
        /// Players inside the room, in the order in which they entered.
        /// </summary>
        [DataMember]
        public List<RoomPlayer> Players { get; set; }

        /// <summary>
        /// Players who left the game in progress and can still come back.
        /// </summary>
        [DataMember]
        public List<RoomPlayer> AbsentPlayers { get; set; }

        /// <summary>
        /// Whether the game of the room already started.
        /// </summary>
        [DataMember]
        public bool IsStarted { get; set; }

        /// <summary>
        /// Options chosen by the host.
        /// </summary>
        [DataMember]
        public RoomSettings Settings { get; set; }
    }
}
