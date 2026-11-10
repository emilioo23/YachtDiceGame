using System;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Callbacks;
using YachtDice.GameServiceReference;
using GameFault = YachtDice.GameServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Keeps the player connected to the game of a room. The server decides the dice, the turns and the
    /// points, so this gateway only sends what the player wants to do and receives the state of the game.
    /// </summary>
    public class GameGateway : DuplexGatewayBase<GameServiceClient>
    {
        private readonly string _playerId;
        private readonly string _roomCode;

        /// <summary>
        /// Initializes the gateway for the player in a room.
        /// </summary>
        /// <param name="playerId">Identifier that the server gave to the player.</param>
        /// <param name="roomCode">Code of the room whose game is played.</param>
        public GameGateway(string playerId, string roomCode)
        {
            _playerId = playerId;
            _roomCode = roomCode;
        }

        /// <summary>
        /// Event triggered when something changes in the game. It can be raised from any thread.
        /// </summary>
        public event Action<GameState, GameEventType> GameChanged;

        /// <summary>
        /// Subscribes the player to the changes of the game and gets the state of the game. If the player
        /// cannot join, the connection is closed.
        /// </summary>
        /// <returns>The outcome and the state of the game; the state is empty if the player could not join.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<JoinGameResult> JoinAsync()
        {
            JoinGameResult result = await ConnectAndExecuteAsync(
                client => client.JoinGame(_playerId, _roomCode));

            if (result.Code != GameResultCode.Success)
            {
                await CloseAsync();
            }

            return result;
        }

        /// <summary>
        /// Asks the server to roll the dice of the turn. The result reaches the player through the changes of
        /// the game.
        /// </summary>
        /// <param name="heldDice">One value for each die: true if the die keeps its value.</param>
        /// <returns>Outcome of the roll.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<GameResultCode> RollDiceAsync(bool[] heldDice)
        {
            GameResultCode result = await ExecuteAsync(
                client => client.RollDice(_playerId, _roomCode, heldDice));

            return result;
        }

        /// <summary>
        /// Asks the server to write the points of the current dice in a box and pass the turn.
        /// </summary>
        /// <param name="category">Box to use.</param>
        /// <returns>Outcome of the move.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task<GameResultCode> SelectCategoryAsync(ScoreCategory category)
        {
            GameResultCode result = await ExecuteAsync(
                client => client.SelectCategory(_playerId, _roomCode, category));

            return result;
        }

        /// <inheritdoc />
        protected override GameServiceClient CreateClient()
        {
            var handler = new GameCallbackHandler();
            handler.GameChanged += HandleGameChanged;

            var client = new GameServiceClient(new InstanceContext(handler));
            ServerAddress.Apply(client.Endpoint);

            return client;
        }

        /// <inheritdoc />
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<GameFault>;

            return GatewayErrorCodeParser.Parse(typedFault?.Detail.Code);
        }

        private void HandleGameChanged(GameState state, GameEventType eventType)
        {
            GameChanged?.Invoke(state, eventType);
        }
    }
}
