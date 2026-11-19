using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using YachtDice.Client.Logic.Connection;
using YachtDice.GameServiceReference;
using YachtDice.Gateway;
using YachtDice.RoomServiceReference;
using YachtDice.Utils;

namespace YachtDice.Navigation
{
    /// <summary>
    /// Gets back the connections of a player to a match in progress when the network fails. The server keeps
    /// the player as absent for a while, so it is enough to connect again with the same room and the same
    /// player: the server gives back the place in the match. The attempts stop when the server stops waiting.
    /// </summary>
    public sealed class GameConnectionRecovery
    {
        private const int MillisecondsPerSecond = 1000;
        private const string NullGatewayMessage = "The connection of the match cannot be null.";
        private const string AttemptFailedLogFormat = "An attempt to reconnect to the match failed: {0}.";
        private const string RestoredLogMessage = "The connection with the match was restored.";

        private readonly RoomGateway _roomGateway;
        private readonly ChatGateway _chat;
        private readonly GameGateway _gameGateway;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly AppLogger _logger;

        /// <summary>
        /// Initializes the recovery of the connections of a player to a match.
        /// </summary>
        /// <param name="roomGateway">Connection of the player to the room.</param>
        /// <param name="chat">Connection of the player to the chat of the room.</param>
        /// <param name="gameGateway">Connection of the player to the game of the room.</param>
        /// <exception cref="ArgumentNullException">Any of the connections is null.</exception>
        public GameConnectionRecovery(RoomGateway roomGateway, ChatGateway chat, GameGateway gameGateway)
        {
            _roomGateway = roomGateway ?? throw new ArgumentNullException(nameof(roomGateway), NullGatewayMessage);
            _chat = chat ?? throw new ArgumentNullException(nameof(chat), NullGatewayMessage);
            _gameGateway = gameGateway ?? throw new ArgumentNullException(nameof(gameGateway), NullGatewayMessage);
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Event triggered when a new attempt to reconnect starts, with the number of the attempt.
        /// </summary>
        public event Action<int> AttemptStarted;

        /// <summary>
        /// Event triggered when the player is connected again, with the state of the match that the server has.
        /// </summary>
        public event Action<GameState> Restored;

        /// <summary>
        /// Tries to connect the player again to the match until it works or the server stops waiting.
        /// </summary>
        /// <returns>True if the player is back in the match; false if the time ran out.</returns>
        public async Task<bool> RecoverAsync()
        {
            var loop = new ReconnectLoop(TryRestoreAsync, WaitAsync, GetElapsedSeconds);
            loop.AttemptStarted += HandleAttemptStarted;
            _stopwatch.Restart();

            bool isRestored = await loop.RunAsync();

            return isRestored;
        }

        private static Task WaitAsync(int seconds)
        {
            return Task.Delay(seconds * MillisecondsPerSecond);
        }

        private int GetElapsedSeconds()
        {
            return (int)_stopwatch.Elapsed.TotalSeconds;
        }

        private void HandleAttemptStarted(int attempt)
        {
            AttemptStarted?.Invoke(attempt);
        }

        private async Task<bool> TryRestoreAsync()
        {
            var isRestored = false;

            AbandonConnections();

            try
            {
                isRestored = await ConnectAgainAsync();
                LogRestoration(isRestored);
            }
            catch (GatewayException ex)
            {
                _logger.Warning(string.Format(CultureInfo.InvariantCulture, AttemptFailedLogFormat, ex.ErrorCode));
            }

            return isRestored;
        }

        private void LogRestoration(bool isRestored)
        {
            if (isRestored)
            {
                _logger.Info(RestoredLogMessage);
            }
        }

        private void AbandonConnections()
        {
            _gameGateway.Abandon();
            _chat.Abandon();
            _roomGateway.Abandon();
        }

        private async Task<bool> ConnectAgainAsync()
        {
            var isRestored = false;

            await _roomGateway.Session.Presence.ConnectAsync(_roomGateway.Session.DisplayName);
            RoomResultCode roomResult = await _roomGateway.JoinRoomAsync(_roomGateway.CurrentRoom.Code);

            if (roomResult == RoomResultCode.Success)
            {
                await _chat.TryJoinAsync();
                isRestored = await JoinGameAsync();
            }

            return isRestored;
        }

        private async Task<bool> JoinGameAsync()
        {
            JoinGameResult result = await _gameGateway.JoinAsync();
            bool isJoined = result.Code == GameResultCode.Success;

            if (isJoined)
            {
                Restored?.Invoke(result.State);
            }

            return isJoined;
        }
    }
}
