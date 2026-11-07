using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Rooms
{
    /// <summary>
    /// Types of events that can happen in a room.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum RoomEventType
    {
        /// <summary>A player entered the room.</summary>
        [EnumMember]
        PlayerJoined,

        /// <summary>A player left the room. If it was the host, another player is the host now.</summary>
        [EnumMember]
        PlayerLeft,

        /// <summary>The host changed the options of the room.</summary>
        [EnumMember]
        SettingsChanged,

        /// <summary>The host started the game.</summary>
        [EnumMember]
        GameStarted
    }
}
