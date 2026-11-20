using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Rooms
{
    /// <summary>
    /// What the player receives after creating a room.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class CreateRoomResult
    {
        /// <summary>
        /// Identifier that the server gave to the player. The player sends it in every later operation.
        /// </summary>
        [DataMember]
        public string PlayerId { get; set; }

        /// <summary>
        /// State of the new room.
        /// </summary>
        [DataMember]
        public RoomState Room { get; set; }
    }
}
