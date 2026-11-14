namespace YachtDice.Client.Logic.Rooms
{
    /// <summary>
    /// Row of the list of rooms: a room that waits for players.
    /// </summary>
    public class RoomListItem
    {
        /// <summary>
        /// Number of characters of the access code of a private room, the same length the server uses.
        /// </summary>
        public const int AccessCodeLength = 6;

        /// <summary>
        /// Gets or sets the identifier of the room inside the list. It is not the code of the room.
        /// </summary>
        public string RoomId { get; set; }

        /// <summary>
        /// Gets or sets the name of the host of the room, which the player can search.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the initial of the name of the host, used for the avatar.
        /// </summary>
        public string Initial { get; set; }

        /// <summary>
        /// Gets or sets the title of the row, which says whose room it is.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the text with the number of players and the speed of the game.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets the text that says whether the room is public or private.
        /// </summary>
        public string PrivacyText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the room is private, so the player needs its code.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the room has a free place.
        /// </summary>
        public bool HasSpace { get; set; }

        /// <summary>
        /// Gets or sets the code that the player typed in the row. It is only used for private rooms.
        /// </summary>
        public string AccessCode { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the other row shows the same room in the same state. The code typed in the row
        /// does not count.
        /// </summary>
        /// <param name="other">Row to compare with.</param>
        /// <returns>True if both rows show the same information.</returns>
        public bool HasSameContentAs(RoomListItem other)
        {
            bool isSameRoom = RoomId == other.RoomId && Title == other.Title;
            bool isSameDescription = Details == other.Details && PrivacyText == other.PrivacyText;
            bool hasSameText = isSameRoom && isSameDescription;

            return hasSameText && HasSpace == other.HasSpace;
        }
    }
}
