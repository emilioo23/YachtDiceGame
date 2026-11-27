namespace YachtDice.Gateway
{
    /// <summary>
    /// Kinds of failures that can happen when talking to the server.
    /// </summary>
    public enum GatewayErrorCode
    {
        /// <summary>A failure that does not fit in any other category.</summary>
        UnexpectedError,

        /// <summary>The server could not be reached or did not answer in time.</summary>
        ServerUnavailable,

        /// <summary>The connection with the server dropped because the network or the server went down.</summary>
        ConnectionLost,

        /// <summary>The server could not reach its database.</summary>
        DatabaseUnavailable,

        /// <summary>The server rejected the request because of invalid data.</summary>
        ValidationFailed,

        /// <summary>
        /// The session of the player ended, so the player must log in again.
        /// </summary>
        SessionExpired,

        /// <summary>
        /// The session of the player ended because the same player logged in again somewhere else.
        /// </summary>
        SessionReplaced
    }
}