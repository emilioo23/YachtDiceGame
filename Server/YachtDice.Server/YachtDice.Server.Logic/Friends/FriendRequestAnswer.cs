namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Answer of a player to a friend request: who sent the request and whether it is accepted.
    /// </summary>
    public class FriendRequestAnswer
    {
        /// <summary>
        /// Gets or sets the name of the player who sent the request.
        /// </summary>
        public string RequesterName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request is accepted; false rejects it.
        /// </summary>
        public bool IsAccepted { get; set; }
    }
}
