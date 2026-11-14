using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Callbacks;
using YachtDice.Client.Logic.Rooms;
using YachtDice.ChatServiceReference;
using YachtDice.Utils;
using ChatFault = YachtDice.ChatServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Keeps the player connected to the chat of a room while the game is open. It also remembers the messages
    /// that arrived, so a window that is opened again can show them. The messages arrive in a network thread
    /// while the windows read them in the thread of the interface. The history and the subscribers change
    /// together, so they need the lock.
    /// </summary>
    public class ChatGateway : DuplexGatewayBase<ChatServiceClient>
    {
        private const string NullMembershipMessage = "The membership of the player in the room cannot be null.";
        private const string JoinFailedLogMessage = "The player could not join the chat of the room.";

        private readonly object _syncRoot = new object();
        private readonly AppLogger _logger;
        private readonly List<ChatMessage> _history = new List<ChatMessage>();
        private readonly RoomMember _member;
        private Action<ChatMessage> _messageReceived;

        /// <summary>
        /// Initializes the gateway for the player in a room.
        /// </summary>
        /// <param name="membership">Player who writes in the chat and the room whose chat is used.</param>
        /// <exception cref="ArgumentNullException">The membership is null.</exception>
        public ChatGateway(RoomMembership membership)
        {
            if (membership == null)
            {
                throw new ArgumentNullException(nameof(membership), NullMembershipMessage);
            }

            _member = new RoomMember
            {
                PlayerId = membership.PlayerId,
                RoomCode = membership.RoomCode
            };
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Tries to register the player in the chat of the room so the server sends the messages to this
        /// computer. The game can be played without chat, so a failure is only written in the log.
        /// </summary>
        /// <returns>True if the player is registered in the chat; false if the chat is not available.</returns>
        public async Task<bool> TryJoinAsync()
        {
            bool isJoined = true;

            try
            {
                await ConnectAndExecuteAsync(client => client.JoinChat(_member));
            }
            catch (GatewayException)
            {
                _logger.Warning(JoinFailedLogMessage);
                isJoined = false;
            }

            return isJoined;
        }

        /// <summary>
        /// Sends a message to every player of the room.
        /// </summary>
        /// <param name="content">Text of the message.</param>
        /// <returns>A task that finishes when the message is sent.</returns>
        /// <exception cref="GatewayException">The server could not be reached.</exception>
        public async Task SendAsync(string content)
        {
            await ExecuteAsync(client => client.SendChatMessage(_member, content));
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
        /// Unsubscribes the player from the chat and closes the connection. If the player never joined the chat
        /// or the connection was already lost, there is nothing to do.
        /// </summary>
        /// <returns>A task that finishes when the player left the chat.</returns>
        /// <exception cref="GatewayException">
        /// The server could not be told. The connection is closed anyway, so the server takes the player out
        /// when it notices.
        /// </exception>
        public async Task LeaveAsync()
        {
            if (IsConnected)
            {
                try
                {
                    await ExecuteAsync(client => client.LeaveChat(_member));
                }
                finally
                {
                    await CloseAsync();
                }
            }
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

            return GatewayErrorCodeParser.Parse(typedFault?.Detail.Code);
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
