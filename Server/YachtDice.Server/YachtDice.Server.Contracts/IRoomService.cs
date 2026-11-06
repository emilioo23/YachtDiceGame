using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Operations that the clients use to create a room, enter it, set it up and start the game.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts, CallbackContract = typeof(IRoomCallback))]
    public interface IRoomService
    {
        /// <summary>
        /// Creates a room with a new code and puts the player inside as its host.
        /// </summary>
        /// <param name="hostName">Name of the player who creates the room.</param>
        /// <returns>The state of the new room.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        RoomState CreateRoom(string hostName);

        /// <summary>
        /// Puts the player inside the room that has the code, if it exists and has space.
        /// </summary>
        /// <param name="playerName">Name of the player who enters.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>Whether the player entered and the state of the room.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        JoinRoomResult JoinRoom(string playerName, string roomCode);

        /// <summary>
        /// Changes the options of the room. Only the host can do it, and only before the game starts.
        /// </summary>
        /// <param name="playerName">Name of the player who asks for the change.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="settings">New options of the room.</param>
        /// <returns>Outcome of the change.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        RoomResultCode UpdateRoomSettings(string playerName, string roomCode, RoomSettings settings);

        /// <summary>
        /// Starts the game of the room and tells every player. Only the host can do it.
        /// </summary>
        /// <param name="playerName">Name of the player who starts the game.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>Outcome of the start.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        RoomResultCode StartGame(string playerName, string roomCode);

        /// <summary>
        /// Takes the player out of the room. If the room stays empty it disappears.
        /// </summary>
        /// <param name="playerName">Name of the player who leaves.</param>
        /// <param name="roomCode">Code of the room.</param>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void LeaveRoom(string playerName, string roomCode);
    }
}
