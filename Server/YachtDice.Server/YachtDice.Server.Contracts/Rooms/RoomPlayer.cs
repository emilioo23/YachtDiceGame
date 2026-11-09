using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Rooms
{
    /// <summary>
    /// A player inside a room, as seen by the clients.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class RoomPlayer
    {
        /// <summary>
        /// Identifier that the server gave to the player when the player entered the room. The clients use it
        /// to know who is who, because two players can show the same name.
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Name that the screens show for the player. It is different for each player of the room.
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }
    }
}
