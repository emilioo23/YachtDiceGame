using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private const string KickLogFormat = "A player was expelled from room {0} by the host.";
        private const int ExpiredRoomsCheckSeconds = 60;
        private const string ClosedRoomLogFormat = "Room {0} was closed because it waited too long to start.";
        private const string CreatedLogFormat = "{0} created room {1}.";
        private const string JoinedLogFormat = "A player entered room {0}.";
        private const string LeftLogFormat = "A player left room {0}.";
        private const string NewHostLogFormat = "A player is now the host of room {0}.";
        private const string NullDependencyMessage = "A dependency of the room service cannot be null.";

        private readonly LogicRooms.IRoomManager _roomManager;
        private readonly GameCoordinator _gameCoordinator;
        private readonly LogicRooms.KickHistoryService _kickHistoryService;

        private readonly CallbackRegistry<Contracts.IRoomCallback> _callbacks =
            new CallbackRegistry<Contracts.IRoomCallback>();

        private TurnClock _expiredRoomsCheck;

        /// <summary>
        /// Initializes a new instance of the RoomService class.
        /// </summary>
        /// <param name="roomManager">The rules and the state of the rooms. It is shared with the chat.</param>
        /// <param name="gameCoordinator">The games of the rooms. It is shared with the game service.</param>
        /// <param name="kickHistoryService">The history where the expulsions are saved.</param>
        public RoomService(
            LogicRooms.IRoomManager roomManager,
            GameCoordinator gameCoordinator,
            LogicRooms.KickHistoryService kickHistoryService)
        {
            _roomManager = roomManager
                ?? throw new ArgumentNullException(nameof(roomManager), NullDependencyMessage);
            _gameCoordinator = gameCoordinator
                ?? throw new ArgumentNullException(nameof(gameCoordinator), NullDependencyMessage);
            _kickHistoryService = kickHistoryService
                ?? throw new ArgumentNullException(nameof(kickHistoryService), NullDependencyMessage);
            _gameCoordinator.ReconnectTimeExpired += ForgetPlayerWhoDidNotReturn;
            _expiredRoomsCheck = StartExpiredRoomsCheck();
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
        /// Gets the latest expulsions where the player was the host or the expelled player.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The expulsions from the newest to the oldest, or an empty list if there is none.</returns>
        public List<Contracts.KickHistoryEntry> GetKickHistory(string sessionToken)
        {
            List<Contracts.KickHistoryEntry> history =
                ExecuteForPlayer(sessionToken, playerName => ProcessKickHistory(playerName));

            return history;
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
        /// <param name="roomAccess">Room of the list that the player wants to enter and its code.</param>
        /// <returns>Whether the player entered, the identifier of the player and the state of the room.</returns>
        public Contracts.JoinRoomResult JoinListedRoom(string displayName, Contracts.RoomAccess roomAccess)
        {
            Contracts.JoinRoomResult result = Execute(roomAccess, () => ProcessJoinListed(displayName, roomAccess));

            return result;
        }

        /// <summary>
        /// Changes the options of the room. Only the host can do it, and only before the game starts.
        /// </summary>
        /// <param name="host">Room and identifier of the player who asks for the change.</param>
        /// <param name="settings">New options of the room.</param>
        /// <returns>Outcome of the change.</returns>
        public Contracts.RoomResultCode UpdateRoomSettings(Contracts.RoomMember host, Contracts.RoomSettings settings)
        {
            LogicRooms.RoomMember member = Mapping.RoomMapper.ToLogic(host);
            Contracts.RoomResultCode result = Execute(settings, () => ProcessSettings(member, settings));

            return result;
        }

        /// <summary>
        /// Starts the game of the room and tells every player. Only the host can do it.
        /// </summary>
        /// <param name="host">Room and identifier of the player who starts the game.</param>
        /// <returns>Outcome of the start.</returns>
        public Contracts.RoomResultCode StartGame(Contracts.RoomMember host)
        {
            LogicRooms.RoomMember member = Mapping.RoomMapper.ToLogic(host);
            Contracts.RoomResultCode result = Execute(host, () => ProcessStart(member));

            return result;
        }

        /// <summary>
        /// Saves whether the player is ready to start and tells every player of the room.
        /// </summary>
        /// <param name="player">Room and identifier of the player.</param>
        /// <param name="isReady">True if the player is ready.</param>
        /// <returns>Outcome of the change.</returns>
        public Contracts.RoomResultCode SetReady(Contracts.RoomMember player, bool isReady)
        {
            LogicRooms.RoomMember member = Mapping.RoomMapper.ToLogic(player);
            Contracts.RoomResultCode result = Execute(player, () => ProcessReady(member, isReady));

            return result;
        }

        /// <summary>
        /// Takes another player out of the room and tells everyone. Only the host can do it, also during the
        /// game. The expelled player cannot come back. The expulsion is written in the log of the server.
        /// </summary>
        /// <param name="host">Room and identifier of the host who expels the player.</param>
        /// <param name="targetPlayerId">Identifier of the player to expel.</param>
        /// <returns>Outcome of the expulsion.</returns>
        public Contracts.RoomResultCode KickPlayer(Contracts.RoomMember host, string targetPlayerId)
        {
            LogicRooms.RoomMember member = Mapping.RoomMapper.ToLogic(host);
            Contracts.RoomResultCode result = Execute(host, () => ProcessKick(member, targetPlayerId));

            return result;
        }

        /// <summary>
        /// Makes another player of the room the host and tells everyone. Only the current host can do it.
        /// </summary>
        /// <param name="host">Room and identifier of the current host.</param>
        /// <param name="newHostId">Identifier of the player who becomes the host.</param>
        /// <returns>Outcome of the change.</returns>
        public Contracts.RoomResultCode TransferHost(Contracts.RoomMember host, string newHostId)
        {
            LogicRooms.RoomMember member = Mapping.RoomMapper.ToLogic(host);
            Contracts.RoomResultCode result = Execute(host, () => ProcessHostTransfer(member, newHostId));

            return result;
        }

        /// <summary>
        /// Adds a computer player to the room and tells everyone. Only the host can do it, before the game.
        /// </summary>
        /// <param name="host">Room and identifier of the host.</param>
        /// <param name="difficulty">How well the computer player plays.</param>
        /// <returns>Outcome of the change.</returns>
        public Contracts.RoomResultCode AddComputer(Contracts.RoomMember host, Contracts.ComputerDifficulty difficulty)
        {
            LogicRooms.RoomMember member = Mapping.RoomMapper.ToLogic(host);
            var logicDifficulty =
                Mapping.EnumMapper.Map<Contracts.ComputerDifficulty, LogicRooms.ComputerDifficulty>(difficulty);
            Contracts.RoomResultCode result = Execute(host, () => ProcessAddComputer(member, logicDifficulty));

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
        /// <param name="member">Room and identifier of the player who leaves.</param>
        public void LeaveRoom(Contracts.RoomMember member)
        {
            LogicRooms.RoomMember leaving = Mapping.RoomMapper.ToLogic(member);

            Execute(member, () => ProcessLeave(leaving));
        }

        private static string Describe(string logFormat, params object[] values)
        {
            return string.Format(CultureInfo.InvariantCulture, logFormat, values);
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
            else if (result.HasHostChanged)
            {
                eventType = Contracts.RoomEventType.HostChanged;
            }

            return eventType;
        }

        private Contracts.CreateRoomResult ProcessCreation(string displayName)
        {
            LogicRooms.RoomResult result = _roomManager.CreateRoom(displayName);

            Subscribe(new LogicRooms.RoomMember(result.Room.Code, result.PlayerId));
            LogInfo(Describe(CreatedLogFormat, DescribePlayer(displayName), result.Room.Code));

            return Mapping.RoomMapper.ToCreateResult(result);
        }

        private Contracts.JoinRoomResult FinishJoin(LogicRooms.RoomResult result)
        {
            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                Subscribe(new LogicRooms.RoomMember(result.Room.Code, result.PlayerId));
                ResumeGameIfReturned(result);
                LogInfo(Describe(JoinedLogFormat, result.Room.Code));
                Broadcast(result.Room, GetJoinEventType(result));
            }

            return Mapping.RoomMapper.ToJoinResult(result);
        }

        private Contracts.JoinRoomResult ProcessJoinListed(string displayName, Contracts.RoomAccess roomAccess)
        {
            LogicRooms.RoomResult result = _roomManager.JoinListed(displayName, Mapping.RoomMapper.ToLogic(roomAccess));

            return FinishJoin(result);
        }

        private Contracts.RoomResultCode ProcessSettings(LogicRooms.RoomMember member, Contracts.RoomSettings settings)
        {
            LogicRooms.RoomResult result = _roomManager.UpdateSettings(member, Mapping.RoomMapper.ToLogic(settings));

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                Broadcast(result.Room, Contracts.RoomEventType.SettingsChanged);
            }

            return Mapping.RoomMapper.ToResultCode(result.Status);
        }

        private Contracts.RoomResultCode ProcessStart(LogicRooms.RoomMember member)
        {
            LogicRooms.RoomResult result = _roomManager.StartGame(member);

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                TellLeftOutPlayers(result);
                _gameCoordinator.StartGame(result.Room);
                Broadcast(result.Room, Contracts.RoomEventType.GameStarted);
            }

            return Mapping.RoomMapper.ToResultCode(result.Status);
        }

        private Contracts.RoomResultCode ProcessAddComputer(
            LogicRooms.RoomMember member,
            LogicRooms.ComputerDifficulty difficulty)
        {
            LogicRooms.RoomResult result = _roomManager.AddComputer(member, difficulty);

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                Broadcast(result.Room, Contracts.RoomEventType.PlayerJoined);
            }

            return Mapping.RoomMapper.ToResultCode(result.Status);
        }

        private Contracts.RoomResultCode ProcessReady(LogicRooms.RoomMember member, bool isReady)
        {
            LogicRooms.RoomResult result = _roomManager.SetReady(member, isReady);

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                Broadcast(result.Room, Contracts.RoomEventType.ReadyChanged);
            }

            return Mapping.RoomMapper.ToResultCode(result.Status);
        }

        private void TellLeftOutPlayers(LogicRooms.RoomResult result)
        {
            foreach (string playerId in result.LeftOutPlayerIds)
            {
                TellRemovedPlayer(
                    result.Room,
                    new LogicRooms.RoomMember(result.Room.Code, playerId),
                    Contracts.RoomEventType.LeftOutOfGame);
            }
        }

        private List<Contracts.KickHistoryEntry> ProcessKickHistory(string displayName)
        {
            Func<LogicRooms.KickHistoryEntry, Contracts.KickHistoryEntry> toContract = entry =>
                new Contracts.KickHistoryEntry
                {
                    HostName = entry.HostName,
                    TargetName = entry.TargetName,
                    RoomCode = entry.RoomCode,
                    Date = entry.Date
                };

            return _kickHistoryService.GetHistory(displayName).Select(toContract).ToList();
        }

        private string ReadDisplayName(LogicRooms.RoomMember member)
        {
            string displayName = string.Empty;

            if (_roomManager.TryGetPlayer(member, out LogicRooms.RoomPlayer player))
            {
                displayName = player.DisplayName;
            }

            return displayName;
        }

        private Contracts.RoomResultCode ProcessKick(LogicRooms.RoomMember member, string targetPlayerId)
        {
            var expelled = new LogicRooms.RoomMember(member.RoomCode, targetPlayerId);
            bool isComputer = IsComputerPlayer(expelled);
            string hostName = ReadDisplayName(member);
            string targetName = ReadDisplayName(expelled);
            LogicRooms.RoomResult result = _roomManager.KickPlayer(member, targetPlayerId);

            if (result.Status == LogicRooms.RoomStatus.Success && isComputer)
            {
                Broadcast(result.Room, Contracts.RoomEventType.SettingsChanged);
            }
            else if (result.Status == LogicRooms.RoomStatus.Success)
            {
                AnnounceExpulsion(result.Room, member, expelled, hostName, targetName);
            }

            return Mapping.RoomMapper.ToResultCode(result.Status);
        }

        private bool IsComputerPlayer(LogicRooms.RoomMember member)
        {
            return _roomManager.TryGetPlayer(member, out LogicRooms.RoomPlayer player) && player.IsComputer;
        }

        private void AnnounceExpulsion(
            LogicRooms.RoomDetails room,
            LogicRooms.RoomMember host,
            LogicRooms.RoomMember expelled,
            string hostName,
            string targetName)
        {
            LogInfo(Describe(KickLogFormat, host.RoomCode));
            TellRemovedPlayer(room, expelled, Contracts.RoomEventType.PlayerKicked);
            _gameCoordinator.PlayerRemoved(expelled);
            Broadcast(room, Contracts.RoomEventType.PlayerKicked);
            _kickHistoryService.Record(host.RoomCode, hostName, targetName);
        }

        private Contracts.RoomResultCode ProcessHostTransfer(LogicRooms.RoomMember member, string newHostId)
        {
            LogicRooms.RoomResult result = _roomManager.TransferHost(member, newHostId);

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                LogInfo(Describe(NewHostLogFormat, member.RoomCode));
                Broadcast(result.Room, Contracts.RoomEventType.HostChanged);
            }

            return Mapping.RoomMapper.ToResultCode(result.Status);
        }

        private void ForgetPlayerWhoDidNotReturn(LogicRooms.RoomMember member)
        {
            LogicRooms.RoomResult result = _roomManager.ForgetAbsentPlayer(member);

            if (result.Status == LogicRooms.RoomStatus.Success
                && result.Room.IsEmpty
                && !result.Room.AbsentPlayers.Any())
            {
                _gameCoordinator.EndGame(member.RoomCode);
            }
        }

        private void TellRemovedPlayer(
            LogicRooms.RoomDetails room,
            LogicRooms.RoomMember removed,
            Contracts.RoomEventType eventType)
        {
            Contracts.RoomState roomState = Mapping.RoomMapper.ToContract(room);

            _callbacks.NotifyPlayer(removed, callback => callback.RoomChanged(roomState, eventType));
            _callbacks.Remove(removed);
        }

        private void ProcessLeave(LogicRooms.RoomMember member)
        {
            _callbacks.Remove(member);
            ApplyLeave(member);
        }

        private void Subscribe(LogicRooms.RoomMember member)
        {
            Contracts.IRoomCallback callback = OperationContext.Current.GetCallbackChannel<Contracts.IRoomCallback>();
            ICommunicationObject channel = OperationContext.Current.Channel;

            _callbacks.Register(member, callback);
            channel.Closed += (sender, e) => HandleChannelEnded(member, callback);
            channel.Faulted += (sender, e) => HandleChannelEnded(member, callback);
        }

        private void HandleChannelEnded(LogicRooms.RoomMember member, Contracts.IRoomCallback callback)
        {
            if (_callbacks.RemoveIfCurrent(member, callback))
            {
                ApplyDisconnect(member);
            }
        }

        private void ApplyDisconnect(LogicRooms.RoomMember member)
        {
            LogicRooms.RoomResult result = _roomManager.Disconnect(member);

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                LogInfo(Describe(LeftLogFormat, member.RoomCode));
                UpdateGameAfterDisconnect(result, member);
            }

            if (result.Status == LogicRooms.RoomStatus.Success && !result.Room.IsEmpty)
            {
                Broadcast(result.Room, GetLeaveEventType(result));
            }
        }

        private void UpdateGameAfterDisconnect(LogicRooms.RoomResult result, LogicRooms.RoomMember member)
        {
            if (result.Room.IsStarted)
            {
                _gameCoordinator.PlayerLeft(member);
            }
            else if (result.Room.IsEmpty)
            {
                _gameCoordinator.EndGame(result.Room.Code);
            }
        }

        private void ApplyLeave(LogicRooms.RoomMember member)
        {
            LogicRooms.RoomResult result = _roomManager.Leave(member);

            if (result.Status == LogicRooms.RoomStatus.Success)
            {
                LogInfo(Describe(LeftLogFormat, member.RoomCode));
                UpdateGameAfterLeave(result, member);
            }

            if (result.Status == LogicRooms.RoomStatus.Success && !result.Room.IsEmpty)
            {
                Broadcast(result.Room, GetLeaveEventType(result));
            }
        }

        private void UpdateGameAfterLeave(LogicRooms.RoomResult result, LogicRooms.RoomMember member)
        {
            if (result.Room.IsEmpty)
            {
                _gameCoordinator.EndGame(result.Room.Code);
            }
            else if (result.Room.IsStarted)
            {
                _gameCoordinator.PlayerLeft(member);
            }
        }

        private void ResumeGameIfReturned(LogicRooms.RoomResult result)
        {
            if (result.HasReturned)
            {
                _gameCoordinator.PlayerReturned(new LogicRooms.RoomMember(result.Room.Code, result.PlayerId));
            }
        }

        private TurnClock StartExpiredRoomsCheck()
        {
            return new TurnClock(TurnClock.NoTurnNumber, ExpiredRoomsCheckSeconds, CloseExpiredRooms);
        }

        private void CloseExpiredRooms()
        {
            try
            {
                foreach (LogicRooms.RoomDetails room in _roomManager.CloseExpiredRooms(DateTime.UtcNow))
                {
                    Broadcast(room, Contracts.RoomEventType.RoomClosed);

                    foreach (LogicRooms.RoomPlayer player in room.Players)
                    {
                        _callbacks.Remove(new LogicRooms.RoomMember(room.Code, player.Id));
                    }

                    LogInfo(string.Format(CultureInfo.InvariantCulture, ClosedRoomLogFormat, room.Code));
                }
            }
            finally
            {
                _expiredRoomsCheck.Dispose();
                _expiredRoomsCheck = StartExpiredRoomsCheck();
            }
        }

        private void Broadcast(LogicRooms.RoomDetails room, Contracts.RoomEventType eventType)
        {
            Contracts.RoomState roomState = Mapping.RoomMapper.ToContract(room);

            _callbacks.Notify(room.Code, callback => callback.RoomChanged(roomState, eventType));
        }
    }
}
