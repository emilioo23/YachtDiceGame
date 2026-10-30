namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Values that the status of a friendship can have in the database.
    /// </summary>
    public static class FriendshipStatus
    {
        /// <summary>The request was sent and has not been answered.</summary>
        public const string Pending = "Pending";

        /// <summary>The request was accepted, so both players are friends.</summary>
        public const string Accepted = "Accepted";
    }
}
