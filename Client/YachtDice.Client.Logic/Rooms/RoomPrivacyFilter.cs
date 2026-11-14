namespace YachtDice.Client.Logic.Rooms
{
    /// <summary>
    /// Kind of rooms that the list of rooms shows.
    /// </summary>
    public enum RoomPrivacyFilter
    {
        /// <summary>Public and private rooms.</summary>
        All,

        /// <summary>Only the public rooms.</summary>
        Public,

        /// <summary>Only the private rooms.</summary>
        Private
    }
}
