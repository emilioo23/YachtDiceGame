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
        public string PlayerId { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the last state of the room that the server sent. It has no code if the player is not in a room.
        /// </summary>
        public RoomState CurrentRoom { get; private set; } = CreateEmptyRoom();

        /// <summary>
        /// Creates a room and puts the player inside as its host.
        /// </summary>
        /// <returns>The state of the new room, with the code that the other players type to enter.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task CreateRoomAsync()
        {
            CreateRoomResult result = await ConnectAndExecuteAsync(
                client => client.CreateRoom(Session.DisplayName));

            PlayerId = result.PlayerId;
            CurrentRoom = result.Room;
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
            JoinRoomResult result = await ConnectAndExecuteAsync(
                client => client.JoinRoom(Session.DisplayName, roomCode));

            return await ApplyJoinResultAsync(result);
        }

        /// <summary>
        /// Puts the player inside a room of the list of rooms. A private room also needs its code. If the
        /// player cannot enter, the connection is closed.
        /// </summary>
        /// <param name="access">Room of the list that the player wants to enter and its code.</param>
        /// <returns>Whether the player entered or why not.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<RoomResultCode> JoinListedRoomAsync(RoomAccess access)
        {
            JoinRoomResult result = await ConnectAndExecuteAsync(
                client => client.JoinListedRoom(Session.DisplayName, access));

            return await ApplyJoinResultAsync(result);
        }

        /// <summary>
        /// Asks the server to change the options of the room. Only the host can do it.
        /// </summary>
        /// <param name="settings">New options of the room.</param>
        /// <returns>Outcome of the change.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<RoomResultCode> UpdateSettingsAsync(RoomSettings settings)
        {
            string playerId = PlayerId;
            string roomCode = CurrentRoom.Code;
            RoomResultCode result = await ExecuteAsync(
                client => client.UpdateRoomSettings(playerId, roomCode, settings));

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
            string playerId = PlayerId;
            string roomCode = CurrentRoom.Code;
            RoomResultCode result = await ExecuteAsync(client => client.StartGame(playerId, roomCode));

            return result;
        }

        /// <summary>
        /// Asks the server to expel another player from the room. Only the host can do it, and only before the
        /// game starts. When it is done, the server tells every player of the room.
        /// </summary>
        /// <param name="targetPlayerId">Identifier of the player to expel.</param>
        /// <returns>Outcome of the expulsion.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<RoomResultCode> KickPlayerAsync(string targetPlayerId)
        {
            string playerId = PlayerId;
            string roomCode = CurrentRoom.Code;
            RoomResultCode result = await ExecuteAsync(
                client => client.KickPlayer(playerId, roomCode, targetPlayerId));

            return result;
        }

        /// <summary>
        /// Asks the server to make another player the host of the room. Only the current host can do it. When
        /// it is done, the server tells every player of the room.
        /// </summary>
        /// <param name="newHostId">Identifier of the player who becomes the host.</param>
        /// <returns>Outcome of the change.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<RoomResultCode> TransferHostAsync(string newHostId)
        {
            string playerId = PlayerId;
            string roomCode = CurrentRoom.Code;
            RoomResultCode result = await ExecuteAsync(
                client => client.TransferHost(playerId, roomCode, newHostId));

            return result;
        }

        /// <summary>
        /// Takes the player out of the room and closes the connection.
        /// </summary>
        /// <returns>A task that finishes when the player left.</returns>
        public async Task LeaveRoomAsync()
        {
            string playerId = PlayerId;
            string roomCode = CurrentRoom.Code;

            try
            {
                await ExecuteAsync(client => client.LeaveRoom(playerId, roomCode));
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
                HostId = string.Empty,
                Players = new RoomPlayer[] { },
                AbsentPlayers = new RoomPlayer[] { },
                Settings = new RoomSettings()
            };

            return room;
        }

        private async Task<RoomResultCode> ApplyJoinResultAsync(JoinRoomResult result)
        {
            if (result.Code == RoomResultCode.Success)
            {
                PlayerId = result.PlayerId;
                CurrentRoom = result.Room;
            }
            else
            {
                await CloseAsync();
            }

            return result.Code;
        }

        private void HandleRoomChanged(RoomState room, RoomEventType eventType)
        {
            CurrentRoom = room;
            RoomChanged?.Invoke(room, eventType);
        }
    }
}
