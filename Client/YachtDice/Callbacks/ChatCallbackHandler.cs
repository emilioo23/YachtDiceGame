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
        /// Raises the message received event when the server delivers a message.
        /// </summary>
        /// <param name="message">The received message data.</param>
        void IChatManagerCallback.MessageReceived(ChatMessage message)
        {
            MessageReceived?.Invoke(message);
        }
    }
}