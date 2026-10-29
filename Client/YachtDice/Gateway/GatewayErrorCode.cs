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

        /// <summary>The server could not reach its database.</summary>
        DatabaseUnavailable,

        /// <summary>The server rejected the request because of invalid data.</summary>
        ValidationFailed
    }
}