using System.Collections.Generic;
using System.ServiceModel;

namespace YachtDice.Server.Contracts.Rooms
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
        /// <param name="displayName">Name that the player who creates the room wants to show.</param>
        /// <returns>The identifier of the player and the state of the new room.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        CreateRoomResult CreateRoom(string displayName);

        /// <summary>
        /// Puts the player inside the room that has the code, if it exists and has space. If another player of
        /// the room already shows the name, the server adds a number to it.
        /// </summary>
        /// <param name="displayName">Name that the player who enters wants to show.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>Whether the player entered, the identifier of the player and the state of the room.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        JoinRoomResult JoinRoom(string displayName, string roomCode);

        /// <summary>
        /// Gets the rooms that wait for players, public and private. The game of a room that already started
        /// is not listed. The codes of the rooms are not sent.
        /// </summary>
        /// <returns>A short description of each room that waits for players.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<RoomSummary> GetRoomList();

        /// <summary>
        /// Puts the player inside a room of the list. A private room also needs its code.
        /// </summary>
        /// <param name="displayName">Name that the player who enters wants to show.</param>
        /// <param name="roomId">Identifier of the room inside the list.</param>
        /// <param name="accessCode">Code of the room. It is only checked if the room is private.</param>
        /// <returns>Whether the player entered, the identifier of the player and the state of the room.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        JoinRoomResult JoinListedRoom(string displayName, string roomId, string accessCode);

        /// <summary>
        /// Changes the options of the room. Only the host can do it, and only before the game starts.
        /// </summary>
        /// <param name="playerId">Identifier of the player who asks for the change.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="settings">New options of the room.</param>
        /// <returns>Outcome of the change.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        RoomResultCode UpdateRoomSettings(string playerId, string roomCode, RoomSettings settings);

        /// <summary>
        /// Starts the game of the room and tells every player. Only the host can do it.
        /// </summary>
        /// <param name="playerId">Identifier of the player who starts the game.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>Outcome of the start.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        RoomResultCode StartGame(string playerId, string roomCode);

        /// <summary>
        /// Takes another player out of the room and tells everyone. Only the host can do it, and only before
        /// the game starts. The expulsion is written in the log of the server.
        /// </summary>
        /// <param name="playerId">Identifier of the host who expels the player.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="targetPlayerId">Identifier of the player to expel.</param>
        /// <returns>Outcome of the expulsion.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        RoomResultCode KickPlayer(string playerId, string roomCode, string targetPlayerId);

        /// <summary>
        /// Makes another player of the room the host and tells everyone. Only the current host can do it.
        /// </summary>
        /// <param name="playerId">Identifier of the current host.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="newHostId">Identifier of the player who becomes the host.</param>
        /// <returns>Outcome of the change.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        RoomResultCode TransferHost(string playerId, string roomCode, string newHostId);

        /// <summary>
        /// Takes the player out of the room. If the player was the host, the next player becomes the host. If
        /// the room stays empty it disappears.
        /// </summary>
        /// <param name="playerId">Identifier of the player who leaves.</param>
        /// <param name="roomCode">Code of the room.</param>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void LeaveRoom(string playerId, string roomCode);
    }
}
