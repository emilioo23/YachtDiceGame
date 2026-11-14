namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Outcome of an operation on a room.
    /// </summary>
    public enum RoomStatus
    {
        /// <summary>The operation was done.</summary>
        Success,

        /// <summary>No room exists with that code, or the player is not inside it.</summary>
        RoomNotFound,

        /// <summary>The room already has the maximum number of players.</summary>
        RoomFull,

        /// <summary>The game of the room already started.</summary>
        GameAlreadyStarted,

        /// <summary>Only the host of the room can do the operation.</summary>
        NotHost,

        /// <summary>The room needs more players to start the game.</summary>
        NotEnoughPlayers,

        /// <summary>The options sent are not valid for the room.</summary>
        InvalidSettings,

        /// <summary>The code sent does not open the private room.</summary>
        InvalidAccessCode,

        /// <summary>The player chosen by the host is not inside the room.</summary>
        PlayerNotFound,

        /// <summary>The host chose themselves, which makes no sense for this operation.</summary>
        InvalidTarget
    }
}
