using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Rooms
{
    /// <summary>
    /// Options of a room that the host can change while the players wait.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class RoomSettings
    {
        /// <summary>
        /// Maximum number of players that fit in the room.
        /// </summary>
        [DataMember]
        public int MaxPlayers { get; set; }

        /// <summary>
        /// Indicates whether the room is private, so only the players who know its code can enter.
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
