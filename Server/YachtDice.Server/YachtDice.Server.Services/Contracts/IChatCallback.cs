using System.ServiceModel;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Notifications that the server sends back to a client subscribed to <see cref="IChatManager"/>.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IChatCallback
    {
        /// <summary>
        /// Called when a new chat message is received.
        /// </summary>
        /// <param name="message">Message that was sent.</param>
        [OperationContract(IsOneWay = true)]
        void OnMessageReceived(ChatMessage message);
    }
}
