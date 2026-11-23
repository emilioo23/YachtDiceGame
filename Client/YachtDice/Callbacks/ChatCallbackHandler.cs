using System;
using System.ServiceModel;
using YachtDice.ChatServiceReference;

namespace YachtDice.Callbacks
{
    /// <summary>
    /// Handles the chat callbacks from the server.
    /// </summary>
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class ChatCallbackHandler : IChatServiceCallback
    {
        /// <summary>
        /// Event triggered when a chat message is received. It is raised from a network thread.
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
