namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Data that identifies a room of the list of rooms and opens the door of it.
    /// </summary>
    public class RoomAccess
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
