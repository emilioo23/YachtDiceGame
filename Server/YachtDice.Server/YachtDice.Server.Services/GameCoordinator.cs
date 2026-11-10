using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Game;
    using LogicGame = YachtDice.Server.Logic.Game;
    using LogicLogging = YachtDice.Server.Logic.Logging;
    using LogicRooms = YachtDice.Server.Logic.Rooms;

    /// <summary>
    /// Connects the games with the rest of the server: it applies the actions of the players, tells every
    /// subscribed player what changed, counts the time of each turn and plays the turn of a player who runs
    /// out of time. The room service and the game service share one coordinator.
    /// </summary>
    public sealed class GameCoordinator
    {
        private const string NullDependencyMessage = "The dependency cannot be null.";
        private const string TimeoutFailedLogMessage = "The turn of a game could not be played after its time ran out.";

        private readonly LogicGame.IGameManager _gameManager;

        private readonly CallbackRegistry<Contracts.IGameCallback> _callbacks =
            new CallbackRegistry<Contracts.IGameCallback>();

        private readonly TurnTimer _timer;
        private readonly LogicLogging.AppLogger _logger;

        /// <summary>
        /// Initializes a coordinator without games.
        /// </summary>
        /// <param name="gameManager">The rules and the state of the games.</param>
        public GameCoordinator(LogicGame.IGameManager gameManager)
        {
            _gameManager = gameManager
                ?? throw new ArgumentNullException(nameof(gameManager), NullDependencyMessage);
            _timer = new TurnTimer(HandleTurnExpired);
            _logger = new LogicLogging.AppLogger(GetType());
        }

        /// <summary>
        /// Creates the game of a room that just started and begins to count the first turn.
        /// </summary>
        /// <param name="room">State of the room, with the players who take part in the game.</param>
        public void StartGame(LogicRooms.RoomDetails room)
        {
            var participants = room.Players
                .Select(player => new LogicGame.GameParticipant(player.Id, player.DisplayName))
                .ToList();
            LogicGame.GameSnapshot snapshot = _gameManager.Start(room.Code, participants, room.Settings.Speed);

            SyncTimer(room.Code, snapshot);
        }

        /// <summary>
        /// Subscribes a player to the changes of the game and gives the current state.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="callback">Callback channel of the player.</param>
        /// <returns>Whether the player joined and the state of the game.</returns>
        public Contracts.JoinGameResult Join(string roomCode, string playerId, Contracts.IGameCallback callback)
        {
            LogicGame.GameActionResult result = _gameManager.GetState(roomCode, playerId);
            var joinResult = new Contracts.JoinGameResult
            {
                Code = Mapping.GameMapper.ToResultCode(result.Status),
                State = Mapping.GameMapper.CreateEmptyState()
            };

            if (result.Status == LogicGame.GameActionStatus.Success)
            {
                _callbacks.Register(roomCode, playerId, callback);
                joinResult.State = Mapping.GameMapper.ToContract(result.Snapshot, _timer.GetSecondsLeft(roomCode));
            }

            return joinResult;
        }

        /// <summary>
        /// Stops notifying a player, only if the channel that is registered is the given one.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="callback">Callback channel that should be the registered one.</param>
        public void Unsubscribe(string roomCode, string playerId, Contracts.IGameCallback callback)
        {
            _callbacks.RemoveIfCurrent(roomCode, playerId, callback);
        }

        /// <summary>
        /// Rolls the dice for a player and tells everyone.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Player who rolls.</param>
        /// <param name="heldDice">One value for each die: true if the die keeps its value.</param>
        /// <returns>Outcome of the roll.</returns>
        public Contracts.GameResultCode Roll(string roomCode, string playerId, IReadOnlyList<bool> heldDice)
        {
            LogicGame.GameActionResult result = _gameManager.Roll(roomCode, playerId, heldDice);

            return Complete(roomCode, result, Contracts.GameEventType.DiceRolled);
        }

        /// <summary>
        /// Writes the points of the dice in a box for a player and tells everyone.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Player who chooses the box.</param>
        /// <param name="category">Box to use.</param>
        /// <returns>Outcome of the move.</returns>
        public Contracts.GameResultCode Score(string roomCode, string playerId, Contracts.ScoreCategory category)
        {
            LogicGame.GameActionResult result =
                _gameManager.Score(roomCode, playerId, Mapping.GameMapper.ToLogic(category));

            return CompleteTurn(roomCode, result, Contracts.GameEventType.CategoryScored);
        }

        /// <summary>
        /// Marks that a player left the game or lost the connection, and tells the others.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Player who left.</param>
        public void PlayerLeft(string roomCode, string playerId)
        {
            _callbacks.Remove(roomCode, playerId);
            Complete(roomCode, _gameManager.MarkAbsent(roomCode, playerId), Contracts.GameEventType.PlayerDisconnected);
        }

        /// <summary>
        /// Marks that a player came back to the game, and tells the others.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Player who returned.</param>
        public void PlayerReturned(string roomCode, string playerId)
        {
            Complete(roomCode, _gameManager.MarkPresent(roomCode, playerId), Contracts.GameEventType.PlayerReturned);
        }

        /// <summary>
        /// Removes a player from the game for good, and tells the others.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Player to remove.</param>
        public void PlayerRemoved(string roomCode, string playerId)
        {
            _callbacks.Remove(roomCode, playerId);
            Complete(roomCode, _gameManager.Remove(roomCode, playerId), Contracts.GameEventType.PlayerRemoved);
        }

        /// <summary>
        /// Deletes the game of a room that does not exist anymore.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        public void EndGame(string roomCode)
        {
            _timer.Stop(roomCode);
            _gameManager.End(roomCode);
        }

        private static Contracts.GameEventType ChooseTurnEvent(
            LogicGame.GameSnapshot snapshot,
            Contracts.GameEventType eventType)
        {
            Contracts.GameEventType chosen = eventType;

            if (snapshot.Status == LogicGame.GameStatus.Finished)
            {
                chosen = Contracts.GameEventType.GameFinished;
            }

            return chosen;
        }

        private Contracts.GameResultCode Complete(
            string roomCode,
            LogicGame.GameActionResult result,
            Contracts.GameEventType eventType)
        {
            if (result.Status == LogicGame.GameActionStatus.Success)
            {
                Publish(roomCode, result.Snapshot, eventType);
            }

            return Mapping.GameMapper.ToResultCode(result.Status);
        }

        private Contracts.GameResultCode CompleteTurn(
            string roomCode,
            LogicGame.GameActionResult result,
            Contracts.GameEventType eventType)
        {
            Contracts.GameEventType chosen = eventType;

            if (result.Status == LogicGame.GameActionStatus.Success)
            {
                chosen = ChooseTurnEvent(result.Snapshot, eventType);
            }

            return Complete(roomCode, result, chosen);
        }

        private void Publish(string roomCode, LogicGame.GameSnapshot snapshot, Contracts.GameEventType eventType)
        {
            SyncTimer(roomCode, snapshot);

            Contracts.GameState state = Mapping.GameMapper.ToContract(snapshot, _timer.GetSecondsLeft(roomCode));

            _callbacks.Notify(roomCode, callback => callback.GameChanged(state, eventType));
        }

        private void SyncTimer(string roomCode, LogicGame.GameSnapshot snapshot)
        {
            var hasTurn = snapshot.Status == LogicGame.GameStatus.InProgress
                && !string.IsNullOrEmpty(snapshot.CurrentPlayerId);

            if (!hasTurn)
            {
                _timer.Stop(roomCode);
            }
            else if (!_timer.IsCounting(roomCode, snapshot.TurnNumber))
            {
                _timer.Start(roomCode, snapshot.TurnNumber, snapshot.TurnSeconds);
            }
        }

        // It runs in a thread of the timer, where an unhandled error would close the whole server.
        private void HandleTurnExpired(string roomCode, int turnNumber)
        {
            try
            {
                LogicGame.GameActionResult result = _gameManager.AutoPlay(roomCode, turnNumber);

                CompleteTurn(roomCode, result, Contracts.GameEventType.TurnTimedOut);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, TimeoutFailedLogMessage);
            }
        }
    }
}
