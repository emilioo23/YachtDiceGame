using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Rooms
{
    /// <summary>
    /// Outcome of the attempt of a player to enter a room.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class JoinRoomResult
    {
        /// <summary>
        /// Whether the player entered or why not.
        /// </summary>
        [DataMember]
        public RoomResultCode Code { get; set; }

        /// <summary>
        /// Identifier that the server gave to the player. It is empty if the player could not enter.
        /// </summary>
        [DataMember]
        public string PlayerId { get; set; }

        /// <summary>
        /// State of the room after the player entered. It has no players if the player could not enter.
        /// </summary>
        [DataMember]
        public RoomState Room { get; set; }
    }
}
