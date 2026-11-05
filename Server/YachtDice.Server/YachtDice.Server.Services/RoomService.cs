using System;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;
    using LogicRooms = YachtDice.Server.Logic.Rooms;

    /// <summary>
    /// Lets the players create rooms, enter them and set them up, and tells every player of a room when
    /// something changes in it.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class RoomService : ServiceBase, Contracts.IRoomService
    {
        private const string NullDependencyMessage = "The dependency cannot be null.";

        private readonly LogicRooms.IRoomManager _roomManager;

        private readonly CallbackRegistry<Contracts.IRoomCallback> _callbacks =
            new CallbackRegistry<Contracts.IRoomCallback>();

        /// <summary>
        /// Initializes a new instance of the RoomService class.
        /// </summary>
        /// <param name="roomManager">The rules and the state of the rooms. It is shared with the chat.</param>
        public RoomService(LogicRooms.IRoomManager roomManager)
        {
            _roomManager = roomManager
                ?? throw new ArgumentNullException(nameof(roomManager), NullDependencyMessage);
        }

        /// <summary>
        /// Creates a room with a new code and puts the player inside as its host.
        /// </summary>
        /// <param name="hostName">Name of the player who creates the room.</param>
        /// <returns>The state of the new room.</returns>
        public Contracts.RoomInfo CreateRoom(string hostName)
        {
            return Execute(hostName, () => ProcessCreation(hostName));
        }

        /// <summary>
        /// Puts the player inside the room that has the code, if it exists and has space.
        /// </summary>
        /// <param name="playerName">Name of the player who enters.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>Whether the player entered and the state of the room.</returns>
        public Contracts.JoinRoomResult JoinRoom(string playerName, string roomCode)
        {
            return Execute(playerName, () => ProcessJoin(playerName, roomCode));
        }

        /// <summary>
        /// Changes the options of the room. Only the host can do it, and only before the game starts.
        /// </summary>
        /// <param name="playerName">Name of the player who asks for the change.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="settings">New options of the room.</param>
        /// <returns>Outcome of the change.</returns>
        public Contracts.RoomResultCode UpdateRoomSettings(
            string playerName,
            string roomCode,
            Contracts.RoomSettings settings)
        {
            return Execute(settings, () => ProcessSettings(playerName, roomCode, settings));
        }

        /// <summary>
        /// Starts the game of the room and tells every player. Only the host can do it.
        /// </summary>
        /// <param name="playerName">Name of the player who starts the game.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>Outcome of the start.</returns>
        public Contracts.RoomResultCode StartGame(string playerName, string roomCode)
        {
            return Execute(playerName, () => ProcessStart(playerName, roomCode));
        }

        /// <summary>
        /// Takes the player out of the room. If the room stays empty it disappears.
        /// </summary>
        /// <param name="playerName">Name of the player who leaves.</param>
        /// <param name="roomCode">Code of the room.</param>
        public void LeaveRoom(string playerName, string roomCode)
        {
            Execute(playerName, () => ProcessLeave(playerName, roomCode));
        }

        private Contracts.RoomInfo ProcessCreation(string hostName)
        {
            LogicRooms.RoomDetails room = _roomManager.CreateRoom(hostName);

            Subscribe(room.Code, hostName);

            return Mapping.RoomMapper.ToContract(room);
        }

        private Contracts.JoinRoomResult ProcessJoin(string playerName, string roomCode)
        {
            LogicRooms.RoomResult result = _roomManager.Join(roomCode, playerName);

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                Subscribe(result.Room.Code, playerName);
                Broadcast(result.Room, Contracts.RoomEventType.PlayerJoined);
            }

            return Mapping.RoomMapper.ToJoinResult(result);
        }

        private Contracts.RoomResultCode ProcessSettings(
            string playerName,
            string roomCode,
            Contracts.RoomSettings settings)
        {
            LogicRooms.RoomResult result =
                _roomManager.UpdateSettings(roomCode, playerName, Mapping.RoomMapper.ToLogic(settings));

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                Broadcast(result.Room, Contracts.RoomEventType.SettingsChanged);
            }

            return Mapping.RoomMapper.ToResultCode(result.Status);
        }

        private Contracts.RoomResultCode ProcessStart(string playerName, string roomCode)
        {
            LogicRooms.RoomResult result = _roomManager.StartGame(roomCode, playerName);

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                Broadcast(result.Room, Contracts.RoomEventType.GameStarted);
            }

            return Mapping.RoomMapper.ToResultCode(result.Status);
        }

        private void ProcessLeave(string playerName, string roomCode)
        {
            _callbacks.Remove(roomCode, playerName);
            ApplyLeave(roomCode, playerName);
        }

        private void Subscribe(string roomCode, string playerName)
        {
            var callback = OperationContext.Current.GetCallbackChannel<Contracts.IRoomCallback>();
            ICommunicationObject channel = OperationContext.Current.Channel;

            _callbacks.Register(roomCode, playerName, callback);

            // The channel can end without the client calling LeaveRoom (crash, network failure), and those
            // events are raised outside the WCF operations. That is why the room state needs its own lock.
            channel.Closed += (sender, e) => HandleChannelEnded(roomCode, playerName, callback);
            channel.Faulted += (sender, e) => HandleChannelEnded(roomCode, playerName, callback);
        }

        private void HandleChannelEnded(string roomCode, string playerName, Contracts.IRoomCallback callback)
        {
            if (_callbacks.RemoveIfCurrent(roomCode, playerName, callback))
            {
                ApplyLeave(roomCode, playerName);
            }
        }

        private void ApplyLeave(string roomCode, string playerName)
        {
            LogicRooms.RoomResult result = _roomManager.Leave(roomCode, playerName);

            if (result.Status == LogicRooms.RoomStatus.Success && !result.Room.IsEmpty)
            {
                Broadcast(result.Room, Contracts.RoomEventType.PlayerLeft);
            }
        }

        private void Broadcast(LogicRooms.RoomDetails room, Contracts.RoomEventType eventType)
        {
            Contracts.RoomInfo info = Mapping.RoomMapper.ToContract(room);

            _callbacks.Notify(room.Code, callback => callback.RoomChanged(info, eventType));
        }
    }
}
