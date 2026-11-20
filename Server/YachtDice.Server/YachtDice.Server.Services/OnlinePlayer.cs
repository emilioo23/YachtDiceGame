using System;
namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Presence;

    /// <summary>
    /// A player who is online, with the session and the callback channel used to reach the player.
    /// </summary>
    internal sealed class OnlinePlayer
    {
        private const string NullSessionIdMessage = "The identifier of the session cannot be null.";
        private const string NullCallbackMessage = "The callback channel of the player cannot be null.";

        /// <summary>
        /// Initializes the player with its session and callback channel.
        /// </summary>
        /// <param name="sessionId">Identifier of the session of the player.</param>
        /// <param name="callback">Callback channel of the player.</param>
        public OnlinePlayer(string sessionId, Contracts.IPresenceCallback callback)
        {
            SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId), NullSessionIdMessage);
            Callback = callback ?? throw new ArgumentNullException(nameof(callback), NullCallbackMessage);
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
