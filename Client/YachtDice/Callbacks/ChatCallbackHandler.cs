using System;
using YachtDice.GameServiceReference;

namespace YachtDice.Callbacks
{
    /// <summary>
    /// Handles chat related callbacks from the server.
    /// </summary>
    public class ChatCallbackHandler : IChatManagerCallback
    {
        /// <summary>
        /// Event triggered when a chat message is received.
        /// </summary>
        public event Action<ChatMessage> MessageReceived;

        /// <summary>
        /// Invokes the message received event.
        /// </summary>
        /// <param name="message">The received message data.</param>
        public void OnMessageReceived(ChatMessage message)
        {
            MessageReceived?.Invoke(message);
        }
    }
}