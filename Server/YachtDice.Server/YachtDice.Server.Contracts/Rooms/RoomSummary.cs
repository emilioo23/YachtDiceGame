using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Rooms
{
    /// <summary>
    /// A room of the list of rooms that the players can browse. It never includes the code of the room, so
    /// a private room can only be entered by the players who know its code.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class RoomSummary
    {
        /// <summary>
        /// Identifier of the room inside the list. It is not the code that the players type.
        /// </summary>
        [DataMember]
        public string RoomId { get; set; }

        /// <summary>
        /// Name that the screens show for the player who controls the room.
        /// </summary>
        [DataMember]
        public string HostName { get; set; }

        /// <summary>
        /// Number of players inside the room.
        /// </summary>
        [DataMember]
        public int PlayerCount { get; set; }

        /// <summary>
        /// Maximum number of players that fit in the room.
        /// </summary>
        [DataMember]
        public int MaxPlayers { get; set; }

        /// <summary>
        /// Indicates whether the room is private, so the player needs its code to enter.
        /// </summary>
        [DataMember]
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Pace of the turns of the game.
        /// </summary>
        [DataMember]
        public GameSpeed Speed { get; set; }
    }
}
