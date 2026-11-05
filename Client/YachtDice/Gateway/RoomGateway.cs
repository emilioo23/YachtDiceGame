using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Callbacks;
using YachtDice.RoomServiceReference;
using YachtDice.Utils;
using RoomErrorCode = YachtDice.RoomServiceReference.ServiceErrorCode;
using RoomFault = YachtDice.RoomServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Keeps the player connected to a room of the server while the lobby and the game are open, and keeps
    /// the last state of the room that the server sent. One gateway belongs to one player in one room.
    /// </summary>
    public class RoomGateway : DuplexGatewayBase<RoomServiceClient>
    {
        private static readonly Dictionary<RoomErrorCode, GatewayErrorCode> _errorCodes =
            new Dictionary<RoomErrorCode, GatewayErrorCode>
            {
                { RoomErrorCode.UnexpectedError, GatewayErrorCode.UnexpectedError },
                { RoomErrorCode.DatabaseUnavailable, GatewayErrorCode.DatabaseUnavailable },
                { RoomErrorCode.ValidationFailed, GatewayErrorCode.ValidationFailed }
            };

        private readonly object _syncRoot = new object();
        private readonly AppLogger _logger = new AppLogger(typeof(RoomGateway));
        private RoomInfo _currentRoom = CreateEmptyRoom();

        /// <summary>
        /// Initializes the gateway for the player.
        /// </summary>
        /// <param name="playerName">Name of the player who uses the room.</param>
        public RoomGateway(string playerName)
        {
            PlayerName = playerName;
        }

        /// <summary>
        /// Event triggered when something changes in the room. It can be raised from any thread.
        /// </summary>
        public event Action<RoomInfo, RoomEventType> RoomChanged;

        /// <summary>
        /// Gets the name of the player who uses the room.
        /// </summary>
        public string PlayerName { get; }

        /// <summary>
        /// Gets the last state of the room that the server sent. It has no code if the player is not in a room.
        /// </summary>
        public RoomInfo CurrentRoom
        {
            get
            {
                RoomInfo room;

                lock (_syncRoot)
                {
                    room = _currentRoom;
                }

                return room;
            }
        }

        /// <summary>
        /// Creates a room and puts the player inside as its host.
        /// </summary>
        /// <returns>The state of the new room, with the code that the other players type to enter.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<RoomInfo> CreateRoomAsync()
        {
            RoomInfo room = await ConnectAndExecuteAsync(client => client.CreateRoom(PlayerName));

            SaveRoom(room);

            return room;
        }

        /// <summary>
        /// Puts the player inside the room that has the code. If the player cannot enter, the connection is
        /// closed.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>Whether the player entered or why not.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<RoomResultCode> JoinRoomAsync(string roomCode)
        {
            JoinRoomResult result = await ConnectAndExecuteAsync(client => client.JoinRoom(PlayerName, roomCode));

            if (result.Code == RoomResultCode.Success)
            {
                SaveRoom(result.Room);
            }
            else
            {
                await CloseAsync();
            }

            return result.Code;
        }

        /// <summary>
        /// Asks the server to change the options of the room. Only the host can do it.
        /// </summary>
        /// <param name="settings">New options of the room.</param>
        /// <returns>Outcome of the change.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public Task<RoomResultCode> UpdateSettingsAsync(RoomSettings settings)
        {
            string roomCode = CurrentRoom.Code;

            return ExecuteAsync(client => client.UpdateRoomSettings(PlayerName, roomCode, settings));
        }

        /// <summary>
        /// Asks the server to start the game. Only the host can do it. When it is done, the server tells every
        /// player of the room.
        /// </summary>
        /// <returns>Outcome of the start.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public Task<RoomResultCode> StartGameAsync()
        {
            string roomCode = CurrentRoom.Code;

            return ExecuteAsync(client => client.StartGame(PlayerName, roomCode));
        }

        /// <summary>
        /// Takes the player out of the room and closes the connection.
        /// </summary>
        /// <returns>A task that finishes when the player left.</returns>
        public async Task LeaveRoomAsync()
        {
            string roomCode = CurrentRoom.Code;

            try
            {
                await ExecuteAsync(client => client.LeaveRoom(PlayerName, roomCode));
            }
            catch (GatewayException ex)
            {
                _logger.Warning(ex.Message);
            }

            await CloseAsync();
        }

        /// <inheritdoc />
        protected override RoomServiceClient CreateClient()
        {
            var handler = new RoomCallbackHandler();
            handler.RoomChanged += HandleRoomChanged;

            var client = new RoomServiceClient(new InstanceContext(handler));
            ServerAddress.Apply(client.Endpoint);

            return client;
        }

        /// <inheritdoc />
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<RoomFault>;
            GatewayErrorCode errorCode;

            if (typedFault == null || !_errorCodes.TryGetValue(typedFault.Detail.Code, out errorCode))
            {
                errorCode = GatewayErrorCode.UnexpectedError;
            }

            return errorCode;
        }

        private static RoomInfo CreateEmptyRoom()
        {
            return new RoomInfo
            {
                Code = string.Empty,
                HostName = string.Empty,
                Players = new string[] { },
                Settings = new RoomSettings()
            };
        }

        private void SaveRoom(RoomInfo room)
        {
            lock (_syncRoot)
            {
                _currentRoom = room;
            }
        }

        private void HandleRoomChanged(RoomInfo room, RoomEventType eventType)
        {
            SaveRoom(room);
            RoomChanged?.Invoke(room, eventType);
        }
    }
}
