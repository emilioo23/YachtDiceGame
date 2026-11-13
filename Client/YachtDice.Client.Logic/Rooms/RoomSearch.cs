namespace YachtDice.Client.Logic.Rooms
{
    /// <summary>
    /// What the player asks of the list of rooms: the kind of rooms and the text to look for.
    /// </summary>
    public sealed class RoomSearch
    {
        /// <summary>
        /// Initializes the search.
        /// </summary>
        /// <param name="privacy">Kind of rooms to keep.</param>
        /// <param name="searchText">Text to look for in the name of the host; empty keeps every row.</param>
        public RoomSearch(RoomPrivacyFilter privacy, string searchText)
        {
            Privacy = privacy;
            SearchText = searchText;
        }

        /// <summary>
        /// Gets the kind of rooms to keep.
        /// </summary>
        public RoomPrivacyFilter Privacy { get; }

        /// <summary>
        /// Gets the text to look for in the name of the host; empty keeps every row.
        /// </summary>
        public string SearchText { get; }
    }
}
