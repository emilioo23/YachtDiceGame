namespace YachtDice.Views
{
    /// <summary>
    /// Simple model to show sample data in the friends list.
    /// </summary>
    public class FriendItem
    {
        /// <summary>
        /// Display name of the friend.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initial of the name, used for the avatar.
        /// </summary>
        public string Initial { get; set; }

        /// <summary>
        /// Current connection or activity status.
        /// </summary>
        public string Status { get; set; }
    }
}