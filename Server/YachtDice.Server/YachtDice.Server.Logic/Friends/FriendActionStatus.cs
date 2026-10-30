namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Outcome of answering a friend request or removing a friend.
    /// </summary>
    public enum FriendActionStatus
    {
        /// <summary>The action was done.</summary>
        Success,

        /// <summary>One of the players does not exist.</summary>
        PlayerNotFound,

        /// <summary>There is no pending request or friendship between the players.</summary>
        RelationNotFound
    }
}
