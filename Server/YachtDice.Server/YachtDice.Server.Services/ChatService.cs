using System;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;
    using LogicLogging = YachtDice.Server.Logic.Logging;
    using LogicRooms = YachtDice.Server.Logic.Rooms;

    /// <summary>
    /// Delivers the chat messages to every player subscribed to a room. Only the players who are inside the
    /// room can write in its chat.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ChatService : ServiceBase, Contracts.IChatService
    {
        private const int MaxMessageLength = 200;
        private const int FirstCharacterIndex = 0;
        private const string NullDependencyMessage = "The dependency cannot be null.";
        private const string IgnoredMessageLogMessage =
            "A chat message was ignored because it is empty or its author is not inside the room.";

        private const string IgnoredJoinLogMessage = "A player who is not inside the room tried to join its chat.";

        private readonly LogicRooms.IRoomManager _roomManager;

        private readonly CallbackRegistry<Contracts.IChatCallback> _callbacks =
            new CallbackRegistry<Contracts.IChatCallback>();

        private readonly LogicLogging.AppLogger _logger = new LogicLogging.AppLogger(typeof(ChatService));

        /// <summary>
        /// Initializes a new instance of the ChatService class.
        /// </summary>
        /// <param name="roomManager">The rules and the state of the rooms. It is shared with the room service.</param>
        public ChatService(LogicRooms.IRoomManager roomManager)
        {
            _roomManager = roomManager
                ?? throw new ArgumentNullException(nameof(roomManager), NullDependencyMessage);
        }

        /// <summary>
        /// Subscribes the player to the messages of the room. The player must already be inside the room.
        /// </summary>
        /// <param name="playerName">Name of the player who joins the chat.</param>
        /// <param name="roomCode">Code of the room.</param>
        public void JoinChat(string playerName, string roomCode)
        {
            Execute(playerName, () => ProcessJoin(playerName, roomCode));
        }

        /// <summary>
        /// Sends a chat message to every player subscribed to the room, including the sender.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="roomCode">Code of the room.</param>
        public void SendChatMessage(Contracts.ChatMessage message, string roomCode)
        {
            if (CanDeliver(message, roomCode))
            {
                Contracts.ChatMessage delivered = CreateDeliveredMessage(message);

                _callbacks.Notify(roomCode, callback => callback.MessageReceived(delivered));
            }
            else
            {
                _logger.Warning(IgnoredMessageLogMessage);
            }
        }

        /// <summary>
        /// Stops sending the messages of the room to the player.
        /// </summary>
        /// <param name="playerName">Name of the player who leaves the chat.</param>
        /// <param name="roomCode">Code of the room.</param>
        public void LeaveChat(string playerName, string roomCode)
        {
            if (!string.IsNullOrWhiteSpace(playerName) && !string.IsNullOrWhiteSpace(roomCode))
            {
                _callbacks.Remove(roomCode, playerName);
            }
        }

        private static Contracts.ChatMessage CreateDeliveredMessage(Contracts.ChatMessage message)
        {
            string content = message.Content.Trim();

            if (content.Length > MaxMessageLength)
            {
                content = content.Substring(FirstCharacterIndex, MaxMessageLength);
            }

            return new Contracts.ChatMessage
            {
                Author = message.Author,
                Content = content,
                Timestamp = DateTime.UtcNow
            };
        }

        private bool CanDeliver(Contracts.ChatMessage message, string roomCode)
        {
            return message != null
                && !string.IsNullOrWhiteSpace(message.Content)
                && _roomManager.IsMember(roomCode, message.Author);
        }

        private void ProcessJoin(string playerName, string roomCode)
        {
            if (_roomManager.IsMember(roomCode, playerName))
            {
                var callback = OperationContext.Current.GetCallbackChannel<Contracts.IChatCallback>();
                ICommunicationObject channel = OperationContext.Current.Channel;

                _callbacks.Register(roomCode, playerName, callback);
                channel.Closed += (sender, e) => HandleChannelEnded(roomCode, playerName, callback);
                channel.Faulted += (sender, e) => HandleChannelEnded(roomCode, playerName, callback);
            }
            else
            {
                _logger.Warning(IgnoredJoinLogMessage);
            }
        }

        private void HandleChannelEnded(string roomCode, string playerName, Contracts.IChatCallback callback)
        {
            _callbacks.RemoveIfCurrent(roomCode, playerName, callback);
        }
    }
}
