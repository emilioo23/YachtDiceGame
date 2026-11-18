namespace YachtDice.Gateway
{
    /// <summary>
    /// Texts of the log and of the exceptions that all the gateways use when a call to the server fails.
    /// </summary>
    internal static class GatewayMessages
    {
        /// <summary>
        /// Text of the log when the server answers with a fault.
        /// </summary>
        public const string FaultLogMessage = "The server reported an error.";

        /// <summary>
        /// Text of the log when the server cannot be reached.
        /// </summary>
        public const string UnavailableLogMessage = "The server could not be reached.";

        /// <summary>
        /// Text of the exception when the server answers with a fault.
        /// </summary>
        public const string FaultMessage = "The server could not complete the request.";

        /// <summary>
        /// Text of the exception when the server cannot be reached.
        /// </summary>
        public const string UnavailableMessage = "The server is not available.";
    }
}
