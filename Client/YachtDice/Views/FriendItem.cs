namespace YachtDice.Views
{
    /// <summary>
    /// Row of the friends list: a friend, or a player who sent a friend request.
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

        /// <summary>
        /// Indicates whether the player is connected to the game right now.
        /// </summary>
        public bool IsOnline { get; set; }
    }
}