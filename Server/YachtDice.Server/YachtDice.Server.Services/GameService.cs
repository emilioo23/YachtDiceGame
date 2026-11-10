using System;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Game;

    /// <summary>
    /// Lets the players of a game in progress roll the dice and choose the boxes of their scorecards. The
    /// server decides the dice, the turns and the points, and tells every player what changed.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class GameService : ServiceBase, Contracts.IGameService
    {
        private const string NullDependencyMessage = "The dependency cannot be null.";

        private readonly GameCoordinator _coordinator;

        /// <summary>
        /// Initializes a new instance of the GameService class.
        /// </summary>
        /// <param name="coordinator">The games and their notifications. It is shared with the room service.</param>
        public GameService(GameCoordinator coordinator)
        {
            _coordinator = coordinator
                ?? throw new ArgumentNullException(nameof(coordinator), NullDependencyMessage);
        }

        /// <summary>
        /// Subscribes the player to the changes of the game of the room and gives the current state.
        /// </summary>
        /// <param name="playerId">Identifier of the player who joins the game.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>Whether the player joined and the state of the game.</returns>
        public Contracts.JoinGameResult JoinGame(string playerId, string roomCode)
        {
            Contracts.JoinGameResult result = Execute(playerId, () => ProcessJoin(playerId, roomCode));

            return result;
        }

        /// <summary>
        /// Rolls the dice of the turn and tells every player.
        /// </summary>
        /// <param name="playerId">Identifier of the player who rolls.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="heldDice">One value for each die: true if the die keeps its value.</param>
        /// <returns>Outcome of the roll.</returns>
        public Contracts.GameResultCode RollDice(string playerId, string roomCode, bool[] heldDice)
        {
            Contracts.GameResultCode result =
                Execute(heldDice, () => _coordinator.Roll(roomCode, playerId, heldDice));

            return result;
        }

        /// <summary>
        /// Writes the points of the current dice in a box and passes the turn to the next player.
        /// </summary>
        /// <param name="playerId">Identifier of the player who chooses the box.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="category">Box to use.</param>
        /// <returns>Outcome of the move.</returns>
        public Contracts.GameResultCode SelectCategory(
            string playerId,
            string roomCode,
            Contracts.ScoreCategory category)
        {
            Contracts.GameResultCode result =
                Execute(playerId, () => _coordinator.Score(roomCode, playerId, category));

            return result;
        }

        private Contracts.JoinGameResult ProcessJoin(string playerId, string roomCode)
        {
            Contracts.IGameCallback callback = OperationContext.Current.GetCallbackChannel<Contracts.IGameCallback>();
            Contracts.JoinGameResult result = _coordinator.Join(roomCode, playerId, callback);

            if (result.Code == Contracts.GameResultCode.Success)
            {
                WatchChannel(roomCode, playerId, callback);
            }

            return result;
        }

        private void WatchChannel(string roomCode, string playerId, Contracts.IGameCallback callback)
        {
            ICommunicationObject channel = OperationContext.Current.Channel;

            channel.Closed += (sender, e) => _coordinator.Unsubscribe(roomCode, playerId, callback);
            channel.Faulted += (sender, e) => _coordinator.Unsubscribe(roomCode, playerId, callback);
        }
    }
}
