using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Operations that the clients can use to send chat messages inside a room.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts, CallbackContract = typeof(IChatCallback))]
    public interface IChatService
    {
        /// <summary>
        /// Sends a chat message to every player connected to a room.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="roomId">Identifier of the room.</param>
        [OperationContract(IsOneWay = true)]
        void SendChatMessage(ChatMessage message, string roomId);
    }
}
