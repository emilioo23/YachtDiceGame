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
        /// the room already shows the name, the server adds a number to it. If the game of the room already
        /// started, only a player who left it can enter again.
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
        /// <param name="roomAccess">Room of the list that the player wants to enter and its code.</param>
        /// <returns>Whether the player entered, the identifier of the player and the state of the room.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        JoinRoomResult JoinListedRoom(string displayName, RoomAccess roomAccess);

        /// <summary>
        /// Changes the options of the room. Only the host can do it, and only before the game starts.
        /// </summary>
        /// <param name="host">Room and identifier of the player who asks for the change.</param>
        /// <param name="settings">New options of the room.</param>
        /// <returns>Outcome of the change.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        RoomResultCode UpdateRoomSettings(RoomMember host, RoomSettings settings);

        /// <summary>
        /// Starts the game of the room and tells every player. Only the host can do it.
        /// </summary>
        /// <param name="host">Room and identifier of the player who starts the game.</param>
        /// <returns>Outcome of the start.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        RoomResultCode StartGame(RoomMember host);

        /// <summary>
        /// Takes another player out of the room and tells everyone. Only the host can do it, also during the
        /// game. The expelled player cannot come back. The expulsion is written in the log of the server.
        /// </summary>
        /// <param name="host">Room and identifier of the host who expels the player.</param>
        /// <param name="targetPlayerId">Identifier of the player to expel.</param>
        /// <returns>Outcome of the expulsion.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        RoomResultCode KickPlayer(RoomMember host, string targetPlayerId);

        /// <summary>
        /// Makes another player of the room the host and tells everyone. Only the current host can do it.
        /// </summary>
        /// <param name="host">Room and identifier of the current host.</param>
        /// <param name="newHostId">Identifier of the player who becomes the host.</param>
        /// <returns>Outcome of the change.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        RoomResultCode TransferHost(RoomMember host, string newHostId);

        /// <summary>
        /// Looks for a game in progress that the player left and can still join again, by using the name of
        /// the player.
        /// </summary>
        /// <param name="displayName">Name that the player shows.</param>
        /// <returns>The code of the room, or an empty string if the player has no pending game.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        string FindPendingRoom(string displayName);

        /// <summary>
        /// Takes the player out of the room. If the player was the host, the next player becomes the host. If
        /// the game already started, the player can come back later. If the room stays empty it disappears.
        /// </summary>
        /// <param name="member">Room and identifier of the player who leaves.</param>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void LeaveRoom(RoomMember member);
    }
}
