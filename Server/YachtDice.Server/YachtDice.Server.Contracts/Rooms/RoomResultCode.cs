using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Rooms
{
    /// <summary>
    /// Outcome of an operation on a room.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum RoomResultCode
    {
        /// <summary>The operation was done.</summary>
        [EnumMember]
        Success,

        /// <summary>No room exists with that code, or the player is not inside it.</summary>
        [EnumMember]
        RoomNotFound,

        /// <summary>The room already has the maximum number of players.</summary>
        [EnumMember]
        RoomFull,

        /// <summary>The game of the room already started.</summary>
        [EnumMember]
        GameAlreadyStarted,

        /// <summary>Only the host of the room can do the operation.</summary>
        [EnumMember]
        NotHost,

        /// <summary>The room needs more players to start the game.</summary>
        [EnumMember]
        NotEnoughPlayers,

        /// <summary>The options sent are not valid for the room.</summary>
        [EnumMember]
        InvalidSettings,

        /// <summary>The code sent does not open the private room.</summary>
        [EnumMember]
        InvalidAccessCode
    }
}
