using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Rooms
{
    /// <summary>
    /// Data that identifies a room of the list of rooms and opens the door of it.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class RoomAccess
    {
        /// <summary>
        /// Identifier of the room inside the list.
        /// </summary>
        [DataMember]
        public string RoomId { get; set; }

        /// <summary>
        /// Code of the room. It is only checked if the room is private.
        /// </summary>
        [DataMember]
        public string AccessCode { get; set; }
    }
}
