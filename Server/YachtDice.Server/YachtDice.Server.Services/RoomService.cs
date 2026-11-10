using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Rooms;
    using LogicRooms = YachtDice.Server.Logic.Rooms;

    /// <summary>
    /// Lets the players create rooms, enter them and set them up, and tells every player of a room when
    /// something changes in it.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class RoomService : ServiceBase, Contracts.IRoomService
    {
        private const string NullDependencyMessage = "The dependency cannot be null.";
        private const string KickLogFormat = "Player {0} was expelled from room {1} by the host {2}.";

        private readonly LogicRooms.IRoomManager _roomManager;
        private readonly GameCoordinator _gameCoordinator;

        private readonly CallbackRegistry<Contracts.IRoomCallback> _callbacks =
            new CallbackRegistry<Contracts.IRoomCallback>();

        /// <summary>
        /// Initializes a new instance of the RoomService class.
        /// </summary>
        /// <param name="roomManager">The rules and the state of the rooms. It is shared with the chat.</param>
        /// <param name="gameCoordinator">The games of the rooms. It is shared with the game service.</param>
        public RoomService(LogicRooms.IRoomManager roomManager, GameCoordinator gameCoordinator)
        {
            _roomManager = roomManager
                ?? throw new ArgumentNullException(nameof(roomManager), NullDependencyMessage);
            _gameCoordinator = gameCoordinator
                ?? throw new ArgumentNullException(nameof(gameCoordinator), NullDependencyMessage);
        }

        /// <summary>
        /// Creates a room with a new code and puts the player inside as its host.
        /// </summary>
        /// <param name="displayName">Name that the player who creates the room wants to show.</param>
        /// <returns>The identifier of the player and the state of the new room.</returns>
        public Contracts.CreateRoomResult CreateRoom(string displayName)
        {
            Contracts.CreateRoomResult result = Execute(displayName, () => ProcessCreation(displayName));

            return result;
        }

        /// <summary>
        /// Puts the player inside the room that has the code, if it exists and has space. If another player of
        /// the room already shows the name, the server adds a number to it.
        /// </summary>
        /// <param name="displayName">Name that the player who enters wants to show.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>Whether the player entered, the identifier of the player and the state of the room.</returns>
        public Contracts.JoinRoomResult JoinRoom(string displayName, string roomCode)
        {
            Contracts.JoinRoomResult result =
                Execute(displayName, () => FinishJoin(_roomManager.Join(roomCode, displayName)));

            return result;
        }

        /// <summary>
        /// Gets the rooms that wait for players, public and private. The codes of the rooms are not sent.
        /// </summary>
        /// <returns>A short description of each room that waits for players.</returns>
        public List<Contracts.RoomSummary> GetRoomList()
        {
            List<Contracts.RoomSummary> rooms =
                Execute(() => Mapping.RoomMapper.ToSummaries(_roomManager.GetOpenRooms()));

            return rooms;
        }

        /// <summary>
        /// Puts the player inside a room of the list. A private room also needs its code.
        /// </summary>
        /// <param name="displayName">Name that the player who enters wants to show.</param>
        /// <param name="roomId">Identifier of the room inside the list.</param>
        /// <param name="accessCode">Code of the room. It is only checked if the room is private.</param>
        /// <returns>Whether the player entered, the identifier of the player and the state of the room.</returns>
        public Contracts.JoinRoomResult JoinListedRoom(string displayName, string roomId, string accessCode)
        {
            Contracts.JoinRoomResult result =
                Execute(displayName, () => FinishJoin(_roomManager.JoinListed(roomId, displayName, accessCode)));

            return result;
        }

        /// <summary>
        /// Changes the options of the room. Only the host can do it, and only before the game starts.
        /// </summary>
        /// <param name="playerId">Identifier of the player who asks for the change.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="settings">New options of the room.</param>
        /// <returns>Outcome of the change.</returns>
        public Contracts.RoomResultCode UpdateRoomSettings(
            string playerId,
            string roomCode,
            Contracts.RoomSettings settings)
        {
            Contracts.RoomResultCode result =
                Execute(settings, () => ProcessSettings(playerId, roomCode, settings));

            return result;
        }

        /// <summary>
        /// Starts the game of the room and tells every player. Only the host can do it.
        /// </summary>
        /// <param name="playerId">Identifier of the player who starts the game.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>Outcome of the start.</returns>
        public Contracts.RoomResultCode StartGame(string playerId, string roomCode)
        {
            Contracts.RoomResultCode result = Execute(playerId, () => ProcessStart(playerId, roomCode));

            return result;
        }

        /// <summary>
        /// Takes another player out of the room and tells everyone. Only the host can do it, also during the
        /// game. The expelled player cannot come back. The expulsion is written in the log of the server.
        /// </summary>
        /// <param name="playerId">Identifier of the host who expels the player.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="targetPlayerId">Identifier of the player to expel.</param>
        /// <returns>Outcome of the expulsion.</returns>
        public Contracts.RoomResultCode KickPlayer(string playerId, string roomCode, string targetPlayerId)
        {
            Contracts.RoomResultCode result =
                Execute(playerId, () => ProcessKick(playerId, roomCode, targetPlayerId));

            return result;
        }

        /// <summary>
        /// Makes another player of the room the host and tells everyone. Only the current host can do it.
        /// </summary>
        /// <param name="playerId">Identifier of the current host.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="newHostId">Identifier of the player who becomes the host.</param>
        /// <returns>Outcome of the change.</returns>
        public Contracts.RoomResultCode TransferHost(string playerId, string roomCode, string newHostId)
        {
            Contracts.RoomResultCode result =
                Execute(playerId, () => ProcessHostTransfer(playerId, roomCode, newHostId));

            return result;
        }

        /// <summary>
        /// Looks for a game in progress that the player left and can still join again, by using the name of
        /// the player.
        /// </summary>
        /// <param name="displayName">Name that the player shows.</param>
        /// <returns>The code of the room, or an empty string if the player has no pending game.</returns>
        public string FindPendingRoom(string displayName)
        {
            string roomCode = Execute(displayName, () => _roomManager.FindPendingRoomCode(displayName));

            return roomCode;
        }

        /// <summary>
        /// Takes the player out of the room. If the player was the host, the next player becomes the host. If
        /// the game already started, the player can come back later. If the room stays empty it disappears.
        /// </summary>
        /// <param name="playerId">Identifier of the player who leaves.</param>
        /// <param name="roomCode">Code of the room.</param>
        public void LeaveRoom(string playerId, string roomCode)
        {
            Execute(playerId, () => ProcessLeave(playerId, roomCode));
        }

        private static Contracts.RoomEventType GetJoinEventType(LogicRooms.RoomResult result)
        {
            Contracts.RoomEventType eventType = Contracts.RoomEventType.PlayerJoined;

            if (result.HasReturned)
            {
                eventType = Contracts.RoomEventType.PlayerReturned;
            }

            return eventType;
        }

        private static Contracts.RoomEventType GetLeaveEventType(LogicRooms.RoomResult result)
        {
            Contracts.RoomEventType eventType = Contracts.RoomEventType.PlayerLeft;

            if (result.Room.IsStarted)
            {
                eventType = Contracts.RoomEventType.PlayerDisconnected;
            }
            else if (result.HostChanged)
            {
                eventType = Contracts.RoomEventType.HostChanged;
            }

            return eventType;
        }

        private Contracts.CreateRoomResult ProcessCreation(string displayName)
        {
            LogicRooms.RoomResult result = _roomManager.CreateRoom(displayName);

            Subscribe(result.Room.Code, result.PlayerId);

            return Mapping.RoomMapper.ToCreateResult(result);
        }

        private Contracts.JoinRoomResult FinishJoin(LogicRooms.RoomResult result)
        {
            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                Subscribe(result.Room.Code, result.PlayerId);
                ResumeGameIfReturned(result);
                Broadcast(result.Room, GetJoinEventType(result));
            }

            return Mapping.RoomMapper.ToJoinResult(result);
        }

        private Contracts.RoomResultCode ProcessSettings(
            string playerId,
            string roomCode,
            Contracts.RoomSettings settings)
        {
            LogicRooms.RoomResult result =
                _roomManager.UpdateSettings(roomCode, playerId, Mapping.RoomMapper.ToLogic(settings));

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                Broadcast(result.Room, Contracts.RoomEventType.SettingsChanged);
            }

            return Mapping.RoomMapper.ToResultCode(result.Status);
        }

        private Contracts.RoomResultCode ProcessStart(string playerId, string roomCode)
        {
            LogicRooms.RoomResult result = _roomManager.StartGame(roomCode, playerId);

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                _gameCoordinator.StartGame(result.Room);
                Broadcast(result.Room, Contracts.RoomEventType.GameStarted);
            }

            return Mapping.RoomMapper.ToResultCode(result.Status);
        }

        private Contracts.RoomResultCode ProcessKick(string playerId, string roomCode, string targetPlayerId)
        {
            LogicRooms.RoomResult result = _roomManager.KickPlayer(roomCode, playerId, targetPlayerId);

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                LogInfo(string.Format(CultureInfo.InvariantCulture, KickLogFormat, targetPlayerId, roomCode, playerId));
                TellExpelledPlayer(result.Room, targetPlayerId);
                _gameCoordinator.PlayerRemoved(roomCode, targetPlayerId);
                Broadcast(result.Room, Contracts.RoomEventType.PlayerKicked);
            }

            return Mapping.RoomMapper.ToResultCode(result.Status);
        }

        private Contracts.RoomResultCode ProcessHostTransfer(string playerId, string roomCode, string newHostId)
        {
            LogicRooms.RoomResult result = _roomManager.TransferHost(roomCode, playerId, newHostId);

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                Broadcast(result.Room, Contracts.RoomEventType.HostChanged);
            }

            return Mapping.RoomMapper.ToResultCode(result.Status);
        }

        // The expelled player gets the notification before it is unsubscribed, because afterwards the room
        // does not send it anything.
        private void TellExpelledPlayer(LogicRooms.RoomDetails room, string targetPlayerId)
        {
            Contracts.RoomState roomState = Mapping.RoomMapper.ToContract(room);

            _callbacks.NotifyPlayer(
                room.Code,
                targetPlayerId,
                callback => callback.RoomChanged(roomState, Contracts.RoomEventType.PlayerKicked));
            _callbacks.Remove(room.Code, targetPlayerId);
        }

        private void ProcessLeave(string playerId, string roomCode)
        {
            _callbacks.Remove(roomCode, playerId);
            ApplyLeave(roomCode, playerId);
        }

        private void Subscribe(string roomCode, string playerId)
        {
            Contracts.IRoomCallback callback = OperationContext.Current.GetCallbackChannel<Contracts.IRoomCallback>();
            ICommunicationObject channel = OperationContext.Current.Channel;

            _callbacks.Register(roomCode, playerId, callback);

            // The channel can end without the client calling LeaveRoom (crash, network failure), and those
            // events are raised outside the WCF operations. That is why the room state needs its own lock.
            channel.Closed += (sender, e) => HandleChannelEnded(roomCode, playerId, callback);
            channel.Faulted += (sender, e) => HandleChannelEnded(roomCode, playerId, callback);
        }

        private void HandleChannelEnded(string roomCode, string playerId, Contracts.IRoomCallback callback)
        {
            if (_callbacks.RemoveIfCurrent(roomCode, playerId, callback))
            {
                ApplyLeave(roomCode, playerId);
            }
        }

        private void ApplyLeave(string roomCode, string playerId)
        {
            LogicRooms.RoomResult result = _roomManager.Leave(roomCode, playerId);

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                UpdateGameAfterLeave(result, playerId);
            }

            if (result.Status == LogicRooms.RoomStatus.Success && !result.Room.IsEmpty)
            {
                Broadcast(result.Room, GetLeaveEventType(result));
            }
        }

        // When the last player leaves, the room disappears and its game has no reason to go on.
        private void UpdateGameAfterLeave(LogicRooms.RoomResult result, string playerId)
        {
            if (result.Room.IsEmpty)
            {
                _gameCoordinator.EndGame(result.Room.Code);
            }
            else if (result.Room.IsStarted)
            {
                _gameCoordinator.PlayerLeft(result.Room.Code, playerId);
            }
        }

        private void ResumeGameIfReturned(LogicRooms.RoomResult result)
        {
            if (result.HasReturned)
            {
                _gameCoordinator.PlayerReturned(result.Room.Code, result.PlayerId);
            }
        }

        private void Broadcast(LogicRooms.RoomDetails room, Contracts.RoomEventType eventType)
        {
            Contracts.RoomState roomState = Mapping.RoomMapper.ToContract(room);

            _callbacks.Notify(room.Code, callback => callback.RoomChanged(roomState, eventType));
        }
    }
}
