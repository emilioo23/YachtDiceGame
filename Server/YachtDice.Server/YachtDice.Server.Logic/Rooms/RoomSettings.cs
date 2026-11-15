namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Options of a room that the host can change while the players wait.
    /// </summary>
    public class RoomSettings
    {
        /// <summary>
        /// Gets or sets the maximum number of players that fit in the room.
        /// </summary>
        public int MaxPlayers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the room is private.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Gets or sets the pace of the turns of the game.
        /// </summary>
        public GameSpeed Speed { get; set; }

        /// <summary>
        /// Gets or sets the name that the host gave to the room; empty if the host did not give one.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
