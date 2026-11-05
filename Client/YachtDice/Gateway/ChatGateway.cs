using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Callbacks;
using YachtDice.ChatServiceReference;
using YachtDice.Utils;
using ChatErrorCode = YachtDice.ChatServiceReference.ServiceErrorCode;
using ChatFault = YachtDice.ChatServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Keeps the player connected to the chat of a room while the game is open. It also remembers the messages
    /// that arrived, so a window that is opened again can show them.
    /// </summary>
    public class ChatGateway : DuplexGatewayBase<ChatServiceClient>
    {
        private static readonly Dictionary<ChatErrorCode, GatewayErrorCode> _errorCodes =
            new Dictionary<ChatErrorCode, GatewayErrorCode>
            {
                { ChatErrorCode.UnexpectedError, GatewayErrorCode.UnexpectedError },
                { ChatErrorCode.DatabaseUnavailable, GatewayErrorCode.DatabaseUnavailable },
                { ChatErrorCode.ValidationFailed, GatewayErrorCode.ValidationFailed }
            };

        private readonly object _syncRoot = new object();
        private readonly AppLogger _logger = new AppLogger(typeof(ChatGateway));
        private readonly List<ChatMessage> _history = new List<ChatMessage>();
        private readonly string _playerName;
        private readonly string _roomCode;
        private Action<ChatMessage> _messageReceived;

        /// <summary>
        /// Initializes the gateway for the player in a room.
        /// </summary>
        /// <param name="playerName">Name of the player who writes in the chat.</param>
        /// <param name="roomCode">Code of the room whose chat is used.</param>
        public ChatGateway(string playerName, string roomCode)
        {
            _playerName = playerName;
            _roomCode = roomCode;
        }

        /// <summary>
        /// Registers the player in the chat of the room so the server sends the messages to this computer.
        /// </summary>
        /// <returns>A task that finishes when the player is registered.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public Task JoinAsync()
        {
            return ConnectAndExecuteAsync(client => client.JoinChat(_playerName, _roomCode));
        }

        /// <summary>
        /// Sends a message to every player of the room.
        /// </summary>
        /// <param name="content">Text of the message.</param>
        /// <returns>A task that finishes when the message is sent.</returns>
        /// <exception cref="GatewayException">The server could not be reached.</exception>
        public Task SendAsync(string content)
        {
            var message = new ChatMessage
            {
                Author = _playerName,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            return ExecuteAsync(client => client.SendChatMessage(message, _roomCode));
        }

        /// <summary>
        /// Starts to give the new messages to the handler and returns the ones that arrived before. A message
        /// is never lost nor given twice: it is either in the returned list or given to the handler. The handler
        /// can be called from any thread.
        /// </summary>
        /// <param name="handler">Method that receives every message that arrives, including the own ones.</param>
        /// <returns>A copy of the messages that arrived before, in order; it is empty if there are none.</returns>
        public List<ChatMessage> Subscribe(Action<ChatMessage> handler)
        {
            List<ChatMessage> history;

            lock (_syncRoot)
            {
                _messageReceived += handler;
                history = new List<ChatMessage>(_history);
            }

            return history;
        }

        /// <summary>
        /// Stops giving the new messages to the handler.
        /// </summary>
        /// <param name="handler">Method that was passed to <see cref="Subscribe"/>.</param>
        public void Unsubscribe(Action<ChatMessage> handler)
        {
            lock (_syncRoot)
            {
                _messageReceived -= handler;
            }
        }

        /// <summary>
        /// Unsubscribes the player from the chat and closes the connection.
        /// </summary>
        /// <returns>A task that finishes when the player left the chat.</returns>
        public async Task LeaveAsync()
        {
            try
            {
                await ExecuteAsync(client => client.LeaveChat(_playerName, _roomCode));
            }
            catch (GatewayException ex)
            {
                _logger.Warning(ex.Message);
            }

            await CloseAsync();
        }

        /// <inheritdoc />
        protected override ChatServiceClient CreateClient()
        {
            var handler = new ChatCallbackHandler();
            handler.MessageReceived += HandleMessageReceived;

            var client = new ChatServiceClient(new InstanceContext(handler));
            ServerAddress.Apply(client.Endpoint);

            return client;
        }

        /// <inheritdoc />
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<ChatFault>;
            GatewayErrorCode errorCode;

            if (typedFault == null || !_errorCodes.TryGetValue(typedFault.Detail.Code, out errorCode))
            {
                errorCode = GatewayErrorCode.UnexpectedError;
            }

            return errorCode;
        }

        private void HandleMessageReceived(ChatMessage message)
        {
            Action<ChatMessage> handler;

            lock (_syncRoot)
            {
                _history.Add(message);
                handler = _messageReceived;
            }

            handler?.Invoke(message);
        }
    }
}
