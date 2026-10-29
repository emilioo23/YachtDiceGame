using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;

    /// <summary>
    /// Delivers the chat messages to every player connected to a room.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ChatService : Contracts.IChatService
    {
        private readonly CallbackRegistry<Contracts.IChatCallback> _callbacks =
            new CallbackRegistry<Contracts.IChatCallback>();

        /// <summary>
        /// Sends a chat message to every client in the room.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="roomId">Identifier of the room.</param>
        public void SendChatMessage(Contracts.ChatMessage message, string roomId)
        {
            _callbacks.Register(roomId, OperationContext.Current.GetCallbackChannel<Contracts.IChatCallback>());
            _callbacks.Notify(roomId, callback => callback.MessageReceived(message));
        }
    }
}
