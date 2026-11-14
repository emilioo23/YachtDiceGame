using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Game;
    using DataExceptions = YachtDice.Server.Data.Exceptions;
    using LogicGame = YachtDice.Server.Logic.Game;
    using LogicLogging = YachtDice.Server.Logic.Logging;
    using LogicRooms = YachtDice.Server.Logic.Rooms;

    /// <summary>
    /// Connects the games with the rest of the server: it applies the actions of the players, tells every
    /// subscribed player what changed, counts the time of each turn and plays the turn of a player who runs
    /// out of time. The room service and the game service share one coordinator. The turn that runs out of
    /// time is played in a thread of the timer, so any error there only goes to the log, because an unhandled
    /// error in that thread would close the whole server.
    /// </summary>
    public sealed class GameCoordinator
    {
        private const string TimeoutFailedLogMessage = "The turn of a game could not be played after its time ran out.";
        private const string ResultNotSavedLogMessage = "The result of a finished game could not be saved.";

        private readonly LogicGame.IGameManager _gameManager;
        private readonly LogicGame.IGameResultRecorder _resultRecorder;

        private readonly ConcurrentDictionary<string, LogicRooms.RoomSettings> _roomSettings =
            new ConcurrentDictionary<string, LogicRooms.RoomSettings>();

        private readonly ConcurrentDictionary<string, bool> _savedGames = new ConcurrentDictionary<string, bool>();

        private readonly CallbackRegistry<Contracts.IGameCallback> _callbacks =
            new CallbackRegistry<Contracts.IGameCallback>();

        private readonly TurnTimer _timer;
        private readonly LogicLogging.AppLogger _logger;

        /// <summary>
        /// Initializes a coordinator without games.
        /// </summary>
        /// <param name="gameManager">The rules and the state of the games.</param>
        /// <param name="resultRecorder">Saves the result of each game that finishes.</param>
        public GameCoordinator(LogicGame.IGameManager gameManager, LogicGame.IGameResultRecorder resultRecorder)
        {
            _gameManager = gameManager
                ?? throw new ArgumentNullException(nameof(gameManager));
            _resultRecorder = resultRecorder
                ?? throw new ArgumentNullException(nameof(resultRecorder));
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

            _roomSettings[room.Code] = room.Settings;
            _savedGames.TryRemove(room.Code, out _);
            SyncTimer(room.Code, snapshot);
        }

        /// <summary>
        /// Subscribes a player to the changes of the game and gives the current state.
        /// </summary>
        /// <param name="member">Room and identifier of the player.</param>
        /// <param name="callback">Callback channel of the player.</param>
        /// <returns>Whether the player joined and the state of the game.</returns>
        public Contracts.JoinGameResult Join(LogicRooms.RoomMember member, Contracts.IGameCallback callback)
        {
            LogicGame.GameActionResult result = _gameManager.GetState(member);
            var joinResult = new Contracts.JoinGameResult
            {
                Code = Mapping.GameMapper.ToResultCode(result.Status),
                State = Mapping.GameMapper.CreateEmptyState()
            };

            if (result.Status == LogicGame.GameActionStatus.Success)
            {
                _callbacks.Register(member, callback);
                joinResult.State =
                    Mapping.GameMapper.ToContract(result.Snapshot, _timer.GetSecondsLeft(member.RoomCode));
            }

            return joinResult;
        }

        /// <summary>
        /// Stops notifying a player, only if the channel that is registered is the given one.
        /// </summary>
        /// <param name="member">Room and identifier of the player.</param>
        /// <param name="callback">Callback channel that should be the registered one.</param>
        public void Unsubscribe(LogicRooms.RoomMember member, Contracts.IGameCallback callback)
        {
            _callbacks.RemoveIfCurrent(member, callback);
        }

        /// <summary>
        /// Rolls the dice for a player and tells everyone.
        /// </summary>
        /// <param name="member">Room and identifier of the player who rolls.</param>
        /// <param name="heldDice">One value for each die: true if the die keeps its value.</param>
        /// <returns>Outcome of the roll.</returns>
        public Contracts.GameResultCode Roll(LogicRooms.RoomMember member, IReadOnlyList<bool> heldDice)
        {
            LogicGame.GameActionResult result = _gameManager.Roll(member, heldDice);

            return Complete(member.RoomCode, result, Contracts.GameEventType.DiceRolled);
        }

        /// <summary>
        /// Writes the points of the dice in a box for a player and tells everyone.
        /// </summary>
        /// <param name="member">Room and identifier of the player who chooses the box.</param>
        /// <param name="category">Box to use.</param>
        /// <returns>Outcome of the move.</returns>
        public Contracts.GameResultCode Score(LogicRooms.RoomMember member, Contracts.ScoreCategory category)
        {
            LogicGame.GameActionResult result = _gameManager.Score(member, Mapping.GameMapper.ToLogic(category));

            return CompleteTurn(member.RoomCode, result, Contracts.GameEventType.CategoryScored);
        }

        /// <summary>
        /// Marks that a player left the game or lost the connection, and tells the others.
        /// </summary>
        /// <param name="member">Room and identifier of the player who left.</param>
        public void PlayerLeft(LogicRooms.RoomMember member)
        {
            _callbacks.Remove(member);
            Complete(member.RoomCode, _gameManager.MarkAbsent(member), Contracts.GameEventType.PlayerDisconnected);
        }

        /// <summary>
        /// Marks that a player came back to the game, and tells the others.
        /// </summary>
        /// <param name="member">Room and identifier of the player who returned.</param>
        public void PlayerReturned(LogicRooms.RoomMember member)
        {
            Complete(member.RoomCode, _gameManager.MarkPresent(member), Contracts.GameEventType.PlayerReturned);
        }

        /// <summary>
        /// Removes a player from the game for good, and tells the others.
        /// </summary>
        /// <param name="member">Room and identifier of the player to remove.</param>
        public void PlayerRemoved(LogicRooms.RoomMember member)
        {
            _callbacks.Remove(member);
            Complete(member.RoomCode, _gameManager.Remove(member), Contracts.GameEventType.PlayerRemoved);
        }

        /// <summary>
        /// Deletes the game of a room that does not exist anymore.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        public void EndGame(string roomCode)
        {
            _timer.Stop(roomCode);
            _gameManager.End(roomCode);
            _roomSettings.TryRemove(roomCode, out _);
            _savedGames.TryRemove(roomCode, out _);
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

            if (chosen == Contracts.GameEventType.GameFinished)
            {
                SaveResult(roomCode, result.Snapshot);
            }

            Contracts.GameResultCode code = Complete(roomCode, result, chosen);

            return code;
        }

        private void SaveResult(string roomCode, LogicGame.GameSnapshot snapshot)
        {
            bool isFirstTime = _savedGames.TryAdd(roomCode, true);

            if (isFirstTime && _roomSettings.TryGetValue(roomCode, out LogicRooms.RoomSettings settings))
            {
                try
                {
                    _resultRecorder.Record(settings, snapshot);
                }
                catch (DataExceptions.DataAccessException ex)
                {
                    _logger.Error(ex, ResultNotSavedLogMessage);
                    snapshot.MarkResultNotSaved();
                }
            }
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
            var turn = new LogicGame.GameTurn(roomCode, snapshot.TurnNumber);

            if (!hasTurn)
            {
                _timer.Stop(roomCode);
            }
            else if (!_timer.IsCounting(turn))
            {
                _timer.Start(turn, snapshot.TurnSeconds);
            }
        }

        private void HandleTurnExpired(LogicGame.GameTurn turn)
        {
            try
            {
                LogicGame.GameActionResult result = _gameManager.AutoPlay(turn);

                CompleteTurn(turn.RoomCode, result, Contracts.GameEventType.TurnTimedOut);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, TimeoutFailedLogMessage);
            }
        }
    }
}
