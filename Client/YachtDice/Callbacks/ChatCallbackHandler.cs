using System;
using YachtDice.ChatServiceReference;

namespace YachtDice.Callbacks
{
    /// <summary>
    /// Handles chat related callbacks from the server.
    /// </summary>
    public class ChatCallbackHandler : IChatServiceCallback
    {
        /// <summary>
        /// Event triggered when a chat message is received.
        /// </summary>
        public event Action<ChatMessage> MessageReceived;

        /// <summary>
        /// Raises the message received event when the server delivers a message.
        /// </summary>
        /// <param name="message">The received message data.</param>
        void IChatServiceCallback.MessageReceived(ChatMessage message)
        {
            MessageReceived?.Invoke(message);
        }
    }
}
