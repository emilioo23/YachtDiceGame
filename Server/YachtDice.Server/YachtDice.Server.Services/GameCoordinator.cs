using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YachtDice.Server.Data.Logging;

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
    /// error in that thread would close the whole server. A player who leaves a game in progress has
    /// <see cref="ReconnectSeconds"/> to come back; after that the player is out of the game and loses.
    /// </summary>
    public sealed class GameCoordinator
    {
        /// <summary>
        /// Seconds that a player who left a game in progress has to come back before losing.
        /// </summary>
        public const int ReconnectSeconds = 60;

        /// <summary>
        /// Seconds that each player of a game restored after a restart of the server has to come back.
        /// </summary>
        public const int RestartReconnectSeconds = 300;

        private const int ComputerMoveSeconds = 2;

        private readonly LogicGame.IGameManager _gameManager;
        private readonly LogicGame.IGameResultRecorder _resultRecorder;
        private readonly LogicGame.IGameCheckpointService _checkpoints;

        private readonly ConcurrentDictionary<string, LogicRooms.RoomSettings> _roomSettings =
            new ConcurrentDictionary<string, LogicRooms.RoomSettings>();

        private readonly ConcurrentDictionary<string, bool> _savedGames = new ConcurrentDictionary<string, bool>();

        private readonly CallbackRegistry<Contracts.IGameCallback> _callbacks =
            new CallbackRegistry<Contracts.IGameCallback>();

        private readonly TurnTimer _timer;
        private readonly TurnTimer _computerTimer;
        private readonly ReconnectTimer _reconnectTimer = new ReconnectTimer();
        private readonly LogicLogging.AppLogger _logger;
        private volatile bool _isStopping;

        /// <summary>
        /// Initializes a coordinator without games.
        /// </summary>
        /// <param name="gameManager">The rules and the state of the games.</param>
        /// <param name="resultRecorder">Saves the result of each game that finishes.</param>
        /// <param name="checkpoints">Saves the games in progress for a restart of the server.</param>
        public GameCoordinator(
            LogicGame.IGameManager gameManager,
            LogicGame.IGameResultRecorder resultRecorder,
            LogicGame.IGameCheckpointService checkpoints)
        {
            _gameManager = gameManager
                ?? throw new ArgumentNullException(nameof(gameManager),
                    LogMessages.GameCoordinatorNullDependencyMessage);
            _resultRecorder = resultRecorder
                ?? throw new ArgumentNullException(nameof(resultRecorder),
                    LogMessages.GameCoordinatorNullDependencyMessage);
            _checkpoints = checkpoints
                ?? throw new ArgumentNullException(nameof(checkpoints),
                    LogMessages.GameCoordinatorNullDependencyMessage);
            _timer = new TurnTimer(HandleTurnExpired);
            _computerTimer = new TurnTimer(HandleComputerMove);
            _logger = new LogicLogging.AppLogger(GetType());
        }

        /// <summary>
        /// Raised when a player did not come back in time and lost, so the room can forget the player.
        /// </summary>
        public event Action<LogicRooms.RoomMember> ReconnectTimeExpired;

        /// <summary>
        /// Creates the game of a room that just started and begins to count the first turn.
        /// </summary>
        /// <param name="room">State of the room, with the players who take part in the game.</param>
        public void StartGame(LogicRooms.RoomDetails room)
        {
            List<LogicGame.GameParticipant> participants = room.Players
                .Select(player => new LogicGame.GameParticipant(player.Id, player.DisplayName)
                {
                    IsComputer = player.IsComputer,
                    Difficulty = player.Difficulty
                })
                .ToList();
            LogicGame.GameSnapshot snapshot = _gameManager.Start(room.Code, participants, room.Settings.Speed);

            _roomSettings[room.Code] = room.Settings;
            _savedGames.TryRemove(room.Code, out _);
            TrackGame(room, snapshot);
            SyncTimer(room.Code, snapshot);
            _logger.Info(string.Format(CultureInfo.InvariantCulture, LogMessages.StartedLogFormat, room.Code,
                participants.Count));
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

            LogicGame.GameActionResult result = _gameManager.MarkAbsent(member);

            Complete(member.RoomCode, result, Contracts.GameEventType.PlayerDisconnected);

            if (result.Status == LogicGame.GameActionStatus.Success
                && !result.Snapshot.IsFinished)
            {
                _reconnectTimer.Start(member, ReconnectSeconds, () => HandleReconnectExpired(member));
            }
        }

        /// <summary>
        /// Marks that a player came back to the game, and tells the others.
        /// </summary>
        /// <param name="member">Room and identifier of the player who returned.</param>
        public void PlayerReturned(LogicRooms.RoomMember member)
        {
            _reconnectTimer.Cancel(member);
            Complete(member.RoomCode, _gameManager.MarkPresent(member), Contracts.GameEventType.PlayerReturned);
        }

        /// <summary>
        /// Removes a player from the game for good, and tells the others.
        /// </summary>
        /// <param name="member">Room and identifier of the player to remove.</param>
        public void PlayerRemoved(LogicRooms.RoomMember member)
        {
            _reconnectTimer.Cancel(member);
            _callbacks.Remove(member);
            CompleteTurn(member.RoomCode, _gameManager.Remove(member), Contracts.GameEventType.PlayerRemoved);
        }

        /// <summary>
        /// Deletes the game of a room that does not exist anymore.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        public void EndGame(string roomCode)
        {
            _timer.Stop(roomCode);
            _computerTimer.Stop(roomCode);
            _reconnectTimer.CancelRoom(roomCode);
            _gameManager.End(roomCode);
            _roomSettings.TryRemove(roomCode, out _);
            _savedGames.TryRemove(roomCode, out _);

            if (!_isStopping)
            {
                UpdateCheckpoint(roomCode, null);
            }
        }

        /// <summary>
        /// Stops saving the games because the server is closing, so the players who get disconnected while it
        /// closes do not change the saved games, which are restored when the server starts again.
        /// </summary>
        public void KeepGamesForRestart()
        {
            _isStopping = true;
        }

        /// <summary>
        /// Puts back the games that were in progress when the server stopped. Each player has
        /// <see cref="RestartReconnectSeconds"/> to come back before losing.
        /// </summary>
        /// <param name="roomManager">The rooms, where the room of each game is put back.</param>
        /// <returns>How many games were restored.</returns>
        /// <exception cref="ArgumentNullException">The room manager is null.</exception>
        public int RestoreSavedGames(LogicRooms.IRoomManager roomManager)
        {
            if (roomManager == null)
            {
                throw new ArgumentNullException(nameof(roomManager), LogMessages.GameCoordinatorNullDependencyMessage);
            }

            int restoredCount = 0;

            try
            {
                foreach (LogicGame.SavedGame saved in _checkpoints.LoadAll())
                {
                    if (roomManager.TryRestoreRoom(saved))
                    {
                        ResumeSavedGame(saved);
                        restoredCount++;
                    }
                }

                _logger.Info(string.Format(CultureInfo.InvariantCulture, LogMessages.GamesRestoredLogFormat,
                    restoredCount));
            }
            catch (DataExceptions.DataAccessException ex)
            {
                _logger.Error(ex, LogMessages.GamesNotRestoredLogMessage);
            }

            return restoredCount;
        }

        private static Contracts.GameEventType ChooseTurnEvent(
            LogicGame.GameSnapshot snapshot,
            Contracts.GameEventType eventType)
        {
            Contracts.GameEventType chosen = eventType;

            if (snapshot.IsFinished)
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
                _reconnectTimer.CancelRoom(roomCode);
                SaveResult(roomCode, result.Snapshot);
            }

            return Complete(roomCode, result, chosen);
        }

        private void SaveResult(string roomCode, LogicGame.GameSnapshot snapshot)
        {
            bool isFirstTime = _savedGames.TryAdd(roomCode, true);

            if (isFirstTime && _roomSettings.TryGetValue(roomCode, out LogicRooms.RoomSettings settings))
            {
                _logger.Info(string.Format(CultureInfo.InvariantCulture, LogMessages.FinishedLogFormat, roomCode));

                try
                {
                    _resultRecorder.Record(settings, snapshot);
                }
                catch (DataExceptions.DataAccessException ex)
                {
                    _logger.Error(ex, LogMessages.ResultNotSavedLogMessage);
                    snapshot.MarkResultNotSaved();
                }
            }
        }

        private void Publish(string roomCode, LogicGame.GameSnapshot snapshot, Contracts.GameEventType eventType)
        {
            SyncTimer(roomCode, snapshot);

            Contracts.GameState state = Mapping.GameMapper.ToContract(snapshot, _timer.GetSecondsLeft(roomCode));

            _callbacks.Notify(roomCode, callback => callback.GameChanged(state, eventType));

            if (!_isStopping)
            {
                UpdateCheckpoint(roomCode, snapshot);
            }
        }

        private void TrackGame(LogicRooms.RoomDetails room, LogicGame.GameSnapshot snapshot)
        {
            try
            {
                if (_checkpoints.Track(room))
                {
                    _checkpoints.Save(room.Code, snapshot);
                }
            }
            catch (DataExceptions.DataAccessException ex)
            {
                _logger.Error(ex, LogMessages.CheckpointNotSavedLogMessage);
            }
        }

        private void UpdateCheckpoint(string roomCode, LogicGame.GameSnapshot snapshot)
        {
            bool isOver = snapshot == null || snapshot.IsFinished;

            try
            {
                if (isOver)
                {
                    _checkpoints.Forget(roomCode);
                }
                else
                {
                    _checkpoints.Save(roomCode, snapshot);
                }
            }
            catch (DataExceptions.DataAccessException ex)
            {
                _logger.Error(ex, LogMessages.CheckpointNotSavedLogMessage);
            }
        }

        private void ResumeSavedGame(LogicGame.SavedGame saved)
        {
            Func<LogicGame.SavedPlayer, bool> isStillPlaying = player => !player.HasForfeited;

            _gameManager.Restore(saved);
            _roomSettings[saved.RoomCode] = saved.Settings;
            _savedGames.TryRemove(saved.RoomCode, out _);

            foreach (LogicGame.SavedPlayer player in saved.Players.Where(isStillPlaying))
            {
                var member = new LogicRooms.RoomMember(saved.RoomCode, player.PlayerId);

                _reconnectTimer.Start(member, RestartReconnectSeconds, () => HandleReconnectExpired(member));
            }
        }

        private void SyncTimer(string roomCode, LogicGame.GameSnapshot snapshot)
        {
            bool hasTurn = !snapshot.IsFinished
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

            SyncComputerTimer(turn, hasTurn && snapshot.IsComputerTurn);
        }

        private void SyncComputerTimer(LogicGame.GameTurn turn, bool isComputerTurn)
        {
            if (isComputerTurn)
            {
                _computerTimer.Start(turn, ComputerMoveSeconds);
            }
            else
            {
                _computerTimer.Stop(turn.RoomCode);
            }
        }

        private void HandleReconnectExpired(LogicRooms.RoomMember member)
        {
            _reconnectTimer.Cancel(member);

            LogicGame.GameActionResult result = _gameManager.Forfeit(member);

            CompleteTurn(member.RoomCode, result, Contracts.GameEventType.PlayerForfeited);

            if (result.Status == LogicGame.GameActionStatus.Success)
            {
                ReconnectTimeExpired?.Invoke(member);
            }
        }

        private void HandleComputerMove(LogicGame.GameTurn turn)
        {
            LogicGame.GameActionResult result = _gameManager.PlayComputerMove(turn);
            bool isSameTurn = result.Status == LogicGame.GameActionStatus.Success
                && result.Snapshot.TurnNumber == turn.TurnNumber;
            Contracts.GameEventType eventType = Contracts.GameEventType.CategoryScored;

            if (isSameTurn)
            {
                eventType = Contracts.GameEventType.DiceRolled;
            }

            CompleteTurn(turn.RoomCode, result, eventType);
        }

        private void HandleTurnExpired(LogicGame.GameTurn turn)
        {
            LogicGame.GameActionResult result = _gameManager.AutoPlay(turn);

            CompleteTurn(turn.RoomCode, result, Contracts.GameEventType.TurnTimedOut);
        }
    }
}
