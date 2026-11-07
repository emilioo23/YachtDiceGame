namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Presence;

    /// <summary>
    /// A player who is online, with the session and the callback channel used to reach the player.
    /// </summary>
    internal sealed class OnlinePlayer
    {
        /// <summary>
        /// Initializes the player with its session and callback channel.
        /// </summary>
        /// <param name="sessionId">Identifier of the session of the player.</param>
        /// <param name="callback">Callback channel of the player.</param>
        public OnlinePlayer(string sessionId, Contracts.IPresenceCallback callback)
        {
            SessionId = sessionId;
            Callback = callback;
        }

        /// <summary>
        /// Gets the identifier of the session of the player.
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// Gets the callback channel of the player.
        /// </summary>
        public Contracts.IPresenceCallback Callback { get; }
    }
}
