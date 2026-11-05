using System.Collections.Generic;
using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Current state of a room, as seen by the clients.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class RoomInfo
    {
        /// <summary>
        /// Code that the players type to enter the room.
        /// </summary>
        [DataMember]
        public string Code { get; set; }

        /// <summary>
        /// Name of the player who controls the room.
        /// </summary>
        [DataMember]
        public string HostName { get; set; }

        /// <summary>
        /// Names of the players inside the room, in the order in which they entered.
        /// </summary>
        [DataMember]
        public List<string> Players { get; set; }

        /// <summary>
        /// Options chosen by the host.
        /// </summary>
        [DataMember]
        public RoomSettings Settings { get; set; }
    }
}
