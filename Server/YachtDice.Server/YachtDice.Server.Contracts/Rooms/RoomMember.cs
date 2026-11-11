using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Rooms
{
    /// <summary>
    /// A player inside a room: the code of the room and the identifier that the room gave to the player. Both
    /// values always travel together in the operations of the rooms, the games, the chat and the invitations.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class RoomMember
    {
        /// <summary>
        /// Identifier that the room gave to the player.
        /// </summary>
        [DataMember]
        public string PlayerId { get; set; }

        /// <summary>
        /// Code of the room.
        /// </summary>
        [DataMember]
        public string RoomCode { get; set; }
    }
}
