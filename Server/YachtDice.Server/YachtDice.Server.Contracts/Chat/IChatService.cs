using System.ServiceModel;

namespace YachtDice.Server.Contracts.Chat
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
        /// <param name="playerId">Identifier of the player who joins the chat.</param>
        /// <param name="roomCode">Code of the room.</param>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void JoinChat(string playerId, string roomCode);

        /// <summary>
        /// Sends a chat message to every player subscribed to the room, including the sender. The server
        /// decides the author from the identifier, so a player cannot write in the name of another one.
        /// </summary>
        /// <param name="playerId">Identifier of the player who writes the message.</param>
        /// <param name="content">Text of the message.</param>
        /// <param name="roomCode">Code of the room.</param>
        [OperationContract(IsOneWay = true)]
        void SendChatMessage(string playerId, string content, string roomCode);

        /// <summary>
        /// Stops sending the messages of the room to the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player who leaves the chat.</param>
        /// <param name="roomCode">Code of the room.</param>
        [OperationContract(IsOneWay = true)]
        void LeaveChat(string playerId, string roomCode);
    }
}
