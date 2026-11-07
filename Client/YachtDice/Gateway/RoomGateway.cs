using System;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Callbacks;
using YachtDice.Models;
using YachtDice.RoomServiceReference;
using YachtDice.Utils;
using RoomFault = YachtDice.RoomServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Keeps the player connected to a room of the server while the lobby and the game are open, and keeps
    /// the last state of the room that the server sent. One gateway belongs to one player in one room.
    /// </summary>
    public class RoomGateway : DuplexGatewayBase<RoomServiceClient>
    {
        private readonly AppLogger _logger;

        /// <summary>
        /// Initializes the gateway for the player.
        /// </summary>
        /// <param name="session">Session of the player who uses the room.</param>
        public RoomGateway(PlayerSession session)
        {
            Session = session;
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Event triggered when something changes in the room. It can be raised from any thread.
        /// </summary>
        public event Action<RoomState, RoomEventType> RoomChanged;

        /// <summary>
        /// Gets the session of the player who uses the room.
        /// </summary>
        public PlayerSession Session { get; }

        /// <summary>
        /// Gets the name of the player who uses the room.
        /// </summary>
        public string PlayerName => Session.DisplayName;

        /// <summary>
        /// Gets the last state of the room that the server sent. It has no code if the player is not in a room.
        /// </summary>
        public RoomState CurrentRoom { get; private set; } = CreateEmptyRoom();

        /// <summary>
        /// Creates a room and puts the player inside as its host.
        /// </summary>
        /// <returns>The state of the new room, with the code that the other players type to enter.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<RoomState> CreateRoomAsync()
        {
            RoomState room = await ConnectAndExecuteAsync(client => client.CreateRoom(PlayerName));

            CurrentRoom = room;

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
                CurrentRoom = result.Room;
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
        public async Task<RoomResultCode> UpdateSettingsAsync(RoomSettings settings)
        {
            string roomCode = CurrentRoom.Code;
            RoomResultCode result = await ExecuteAsync(
                client => client.UpdateRoomSettings(PlayerName, roomCode, settings));

            return result;
        }

        /// <summary>
        /// Asks the server to start the game. Only the host can do it. When it is done, the server tells every
        /// player of the room.
        /// </summary>
        /// <returns>Outcome of the start.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<RoomResultCode> StartGameAsync()
        {
            string roomCode = CurrentRoom.Code;
            RoomResultCode result = await ExecuteAsync(client => client.StartGame(PlayerName, roomCode));

            return result;
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

            return GatewayErrorCodeParser.Parse(typedFault?.Detail.Code);
        }

        private static RoomState CreateEmptyRoom()
        {
            var room = new RoomState
            {
                Code = string.Empty,
                HostName = string.Empty,
                Players = new string[] { },
                Settings = new RoomSettings()
            };

            return room;
        }

        private void HandleRoomChanged(RoomState room, RoomEventType eventType)
        {
            CurrentRoom = room;
            RoomChanged?.Invoke(room, eventType);
        }
    }
}
