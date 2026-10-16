using System.ServiceModel;
using System.Threading.Tasks;

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
        /// <returns>A task that completes when the message has been broadcast.</returns>
        [OperationContract]
        Task SendMessageAsync(ChatMessage message, string roomId);
    }
}
