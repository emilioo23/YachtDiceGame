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
        public GameActionResult GetState(RoomMember member)
        {

            return Execute(member.RoomCode, game => CheckMember(game, member.PlayerId));
        }

        /// <inheritdoc />
        public GameActionResult Roll(RoomMember member, IReadOnlyList<bool> heldDice)
        {

            return Execute(member.RoomCode, game => game.Roll(member.PlayerId, heldDice));
        }

        /// <inheritdoc />
        public GameActionResult Score(RoomMember member, ScoreCategory category)
        {

            return Execute(member.RoomCode, game => game.Score(member.PlayerId, category));
        }

        /// <inheritdoc />
        public GameActionResult AutoPlay(string roomCode, int turnNumber)
        {

            return Execute(roomCode, game => AutoPlayIfSameTurn(game, turnNumber));
        }

        /// <inheritdoc />
        public GameActionResult MarkAbsent(RoomMember member)
        {

            return Execute(member.RoomCode, game => ChangePresence(game, member.PlayerId, false));
        }

        /// <inheritdoc />
        public GameActionResult MarkPresent(RoomMember member)
        {

            return Execute(member.RoomCode, game => ChangePresence(game, member.PlayerId, true));
        }

        /// <inheritdoc />
        public GameActionResult Remove(RoomMember member)
        {

            return Execute(member.RoomCode, game => RemovePlayer(game, member.PlayerId));
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
