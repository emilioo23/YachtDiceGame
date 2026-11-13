namespace YachtDice.Models
{
    /// <summary>
    /// How a friend is shown in a list: the text of its status and whether the friend is online.
    /// </summary>
    public sealed class FriendPresence
    {
        /// <summary>
        /// Initializes the presence of a friend.
        /// </summary>
        /// <param name="status">Text that describes the status of the friend.</param>
        /// <param name="isOnline">Whether the friend is online.</param>
        public FriendPresence(string status, bool isOnline)
        {
            Status = status;
            IsOnline = isOnline;
        }

        /// <summary>
        /// Gets the text that describes the status of the friend.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// Gets a value indicating whether the friend is online.
        /// </summary>
        public bool IsOnline { get; }
    }
}
