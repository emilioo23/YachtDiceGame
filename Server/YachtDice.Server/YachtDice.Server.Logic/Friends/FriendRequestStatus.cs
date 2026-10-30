namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Outcome of the request to add a friend by its friend code.
    /// </summary>
    public enum FriendRequestStatus
    {
        /// <summary>The request was sent and the other player has to accept it.</summary>
        RequestSent,

        /// <summary>The other player had already sent a request, so both players are friends now.</summary>
        FriendAdded,

        /// <summary>The friend code is empty.</summary>
        InvalidCode,

        /// <summary>No player uses the friend code.</summary>
        CodeNotFound,

        /// <summary>The friend code belongs to the player who sends the request.</summary>
        SelfRequest,

        /// <summary>The players are already friends.</summary>
        AlreadyFriends,

        /// <summary>The player already sent a request that has not been answered.</summary>
        AlreadyRequested,

        /// <summary>The player who sends the request does not exist.</summary>
        PlayerNotFound
    }
}
