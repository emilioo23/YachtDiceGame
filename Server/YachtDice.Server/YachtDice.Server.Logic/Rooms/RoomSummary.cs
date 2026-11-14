namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Short description of a room for the list of rooms. It does not include the code of the room.
    /// </summary>
    public class RoomSummary
    {
        /// <summary>
        /// Gets the identifier of the room inside the list. It is not the code that the players type.
        /// </summary>
        public string RoomId { get; internal set; }

        /// <summary>
        /// Gets the name that the screens show for the player who controls the room.
        /// </summary>
        public string HostName { get; internal set; }

        /// <summary>
        /// Gets the number of players inside the room.
        /// </summary>
        public int PlayerCount { get; internal set; }

        /// <summary>
        /// Gets the maximum number of players that fit in the room.
        /// </summary>
        public int MaxPlayers { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the room is private.
        /// </summary>
        public bool IsPrivate { get; internal set; }

        /// <summary>
        /// Gets the pace of the turns of the game.
        /// </summary>
        public GameSpeed Speed { get; internal set; }
    }
}
