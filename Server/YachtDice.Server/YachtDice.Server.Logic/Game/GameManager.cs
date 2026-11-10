using System;
using System.Collections.Generic;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Keeps the games that are being played and runs each action while it holds a lock, so two players
    /// can never change the same game at the same time.
    /// </summary>
    public sealed class GameManager : IGameManager
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, YachtGame> _games =
            new Dictionary<string, YachtGame>(StringComparer.OrdinalIgnoreCase);
        private readonly IDieSource _dieSource;

        /// <summary>
        /// Initializes a manager without games.
        /// </summary>
        /// <param name="dieSource">Source of the values of the dice.</param>
        public GameManager(IDieSource dieSource)
        {
            _dieSource = dieSource;
        }

        /// <inheritdoc />
        public GameSnapshot Start(string roomCode, IEnumerable<GameParticipant> participants, GameSpeed speed)
        {
            var game = new YachtGame(participants, _dieSource, TurnDuration.GetSeconds(speed));

            GameSnapshot snapshot;

            lock (_syncRoot)
            {
                _games[roomCode] = game;
                snapshot = game.CreateSnapshot();
            }

            return snapshot;
        }

        /// <inheritdoc />
        public GameActionResult GetState(string roomCode, string playerId)
        {
            return Execute(roomCode, game => CheckMember(game, playerId));
        }

        /// <inheritdoc />
        public GameActionResult Roll(string roomCode, string playerId, IReadOnlyList<bool> heldDice)
        {
            return Execute(roomCode, game => game.Roll(playerId, heldDice));
        }

        /// <inheritdoc />
        public GameActionResult Score(string roomCode, string playerId, ScoreCategory category)
        {
            return Execute(roomCode, game => game.Score(playerId, category));
        }

        /// <inheritdoc />
        public GameActionResult AutoPlay(string roomCode, int turnNumber)
        {
            return Execute(roomCode, game => AutoPlayIfSameTurn(game, turnNumber));
        }

        /// <inheritdoc />
        public GameActionResult MarkAbsent(string roomCode, string playerId)
        {
            return Execute(roomCode, game => ChangePresence(game, playerId, false));
        }

        /// <inheritdoc />
        public GameActionResult MarkPresent(string roomCode, string playerId)
        {
            return Execute(roomCode, game => ChangePresence(game, playerId, true));
        }

        /// <inheritdoc />
        public GameActionResult Remove(string roomCode, string playerId)
        {
            return Execute(roomCode, game => RemovePlayer(game, playerId));
        }

        /// <inheritdoc />
        public void End(string roomCode)
        {
            lock (_syncRoot)
            {
                _games.Remove(roomCode);
            }
        }

        private static GameActionStatus CheckMember(YachtGame game, string playerId)
        {
            GameActionStatus status;

            if (game.HasPlayer(playerId))
            {
                status = GameActionStatus.Success;
            }
            else
            {
                status = GameActionStatus.PlayerNotInGame;
            }

            return status;
        }

        private static GameActionStatus AutoPlayIfSameTurn(YachtGame game, int turnNumber)
        {
            GameActionStatus status;

            if (game.TurnNumber == turnNumber)
            {
                status = game.AutoPlay();
            }
            else
            {
                status = GameActionStatus.NotYourTurn;
            }

            return status;
        }

        private static GameActionStatus ChangePresence(YachtGame game, string playerId, bool isPresent)
        {
            var status = CheckMember(game, playerId);

            if (status == GameActionStatus.Success && isPresent)
            {
                game.MarkPresent(playerId);
            }
            else if (status == GameActionStatus.Success)
            {
                game.MarkAbsent(playerId);
            }

            return status;
        }

        private static GameActionStatus RemovePlayer(YachtGame game, string playerId)
        {
            var status = CheckMember(game, playerId);

            if (status == GameActionStatus.Success)
            {
                game.Remove(playerId);
            }

            return status;
        }

        private GameActionResult Execute(string roomCode, Func<YachtGame, GameActionStatus> action)
        {
            GameActionResult result;

            lock (_syncRoot)
            {
                result = RunAction(roomCode, action);
            }

            return result;
        }

        private GameActionResult RunAction(string roomCode, Func<YachtGame, GameActionStatus> action)
        {
            GameActionResult result;

            if (_games.ContainsKey(roomCode))
            {
                var game = _games[roomCode];
                var status = action(game);

                if (status == GameActionStatus.Success)
                {
                    result = GameActionResult.CreateSucceeded(game.CreateSnapshot());
                }
                else
                {
                    result = GameActionResult.CreateFailed(status);
                }
            }
            else
            {
                result = GameActionResult.CreateFailed(GameActionStatus.GameNotFound);
            }

            return result;
        }
    }
}
