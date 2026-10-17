using System.ServiceModel;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Operations that the clients can use to send chat messages inside a room.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts, CallbackContract = typeof(IChatCallback))]
    public interface IChatManager
    {
        /// <summary>
        /// Sends a chat message to every player connected to a room.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="roomId">Identifier of the room.</param>
        [OperationContract(IsOneWay = true)]
        void SendMessage(ChatMessage message, string roomId);
    }
}