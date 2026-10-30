namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Result of the request to add a friend by its friend code.
    /// </summary>
    public class FriendRequestResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FriendRequestResult"/> class.
        /// </summary>
        /// <param name="status">Outcome of the request.</param>
        /// <param name="friendName">Name of the player who owns the friend code.</param>
        public FriendRequestResult(FriendRequestStatus status, string friendName)
        {
            Status = status;
            FriendName = friendName;
        }

        /// <summary>
        /// Gets the outcome of the request.
        /// </summary>
        public FriendRequestStatus Status { get; }

        /// <summary>
        /// Gets the name of the player who owns the friend code; empty if no player was found.
        /// </summary>
        public string FriendName { get; }
    }
}
