using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Operations that the clients use to talk with the other players of their room.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts, CallbackContract = typeof(IChatCallback))]
    public interface IChatService
    {
        /// <summary>
        /// Subscribes the player to the messages of the room. The player must already be inside the room.
        /// </summary>
        /// <param name="playerName">Name of the player who joins the chat.</param>
        /// <param name="roomCode">Code of the room.</param>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void JoinChat(string playerName, string roomCode);

        /// <summary>
        /// Sends a chat message to every player subscribed to the room, including the sender.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="roomCode">Code of the room.</param>
        [OperationContract(IsOneWay = true)]
        void SendChatMessage(ChatMessage message, string roomCode);

        /// <summary>
        /// Stops sending the messages of the room to the player.
        /// </summary>
        /// <param name="playerName">Name of the player who leaves the chat.</param>
        /// <param name="roomCode">Code of the room.</param>
        [OperationContract(IsOneWay = true)]
        void LeaveChat(string playerName, string roomCode);
    }
}
