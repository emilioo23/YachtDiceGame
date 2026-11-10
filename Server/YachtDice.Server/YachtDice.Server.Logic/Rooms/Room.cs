namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Room of the list of rooms that a player wants to enter, with the code that opens its door.
    /// </summary>
    public class Room
    {
        /// <summary>
        /// Gets or sets the identifier of the room inside the list.
        /// </summary>
        public string RoomId { get; set; }

        /// <summary>
        /// Gets or sets the code of the room. It is only checked if the room is private.
        /// </summary>
        public string AccessCode { get; set; }
    }
}
