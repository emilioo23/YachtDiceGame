using System;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Chat;
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

        private const string NotInsideRoomMessage = "A player who is not inside the room cannot join its chat.";

        private readonly LogicRooms.IRoomManager _roomManager;

        private readonly CallbackRegistry<Contracts.IChatCallback> _callbacks =
            new CallbackRegistry<Contracts.IChatCallback>();

        private readonly LogicLogging.AppLogger _logger;

        /// <summary>
        /// Initializes a new instance of the ChatService class.
        /// </summary>
        /// <param name="roomManager">The rules and the state of the rooms. It is shared with the room service.</param>
        public ChatService(LogicRooms.IRoomManager roomManager)
        {
            _roomManager = roomManager
                ?? throw new ArgumentNullException(nameof(roomManager), NullDependencyMessage);
            _logger = new LogicLogging.AppLogger(GetType());
        }

        /// <summary>
        /// Subscribes the player to the messages of the room. The player must already be inside the room.
        /// </summary>
        /// <param name="playerId">Identifier of the player who joins the chat.</param>
        /// <param name="roomCode">Code of the room.</param>
        public void JoinChat(string playerId, string roomCode)
        {
            Execute(playerId, () => ProcessJoin(playerId, roomCode));
        }

        /// <summary>
        /// Sends a chat message to every player subscribed to the room, including the sender. The author is
        /// the player that has the identifier, so a player cannot write in the name of another one.
        /// </summary>
        /// <param name="playerId">Identifier of the player who writes the message.</param>
        /// <param name="content">Text of the message.</param>
        /// <param name="roomCode">Code of the room.</param>
        public void SendChatMessage(string playerId, string content, string roomCode)
        {
            if (!string.IsNullOrWhiteSpace(content)
                && _roomManager.TryGetPlayer(roomCode, playerId, out LogicRooms.RoomPlayer author))
            {
                Contracts.ChatMessage delivered = CreateDeliveredMessage(author, content);

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
        /// <param name="playerId">Identifier of the player who leaves the chat.</param>
        /// <param name="roomCode">Code of the room.</param>
        public void LeaveChat(string playerId, string roomCode)
        {
            if (!string.IsNullOrWhiteSpace(playerId) && !string.IsNullOrWhiteSpace(roomCode))
            {
                _callbacks.Remove(roomCode, playerId);
            }
        }

        private static Contracts.ChatMessage CreateDeliveredMessage(LogicRooms.RoomPlayer author, string content)
        {
            string text = content.Trim();

            if (text.Length > MaxMessageLength)
            {
                text = text.Substring(FirstCharacterIndex, MaxMessageLength);
            }

            var delivered = new Contracts.ChatMessage
            {
                AuthorId = author.Id,
                Author = author.DisplayName,
                Content = text,
                Timestamp = DateTime.UtcNow
            };

            return delivered;
        }

        private void ProcessJoin(string playerId, string roomCode)
        {
            if (_roomManager.TryGetPlayer(roomCode, playerId, out _))
            {
                Contracts.IChatCallback callback =
                    OperationContext.Current.GetCallbackChannel<Contracts.IChatCallback>();
                ICommunicationObject channel = OperationContext.Current.Channel;

                _callbacks.Register(roomCode, playerId, callback);
                channel.Closed += (sender, e) => HandleChannelEnded(roomCode, playerId, callback);
                channel.Faulted += (sender, e) => HandleChannelEnded(roomCode, playerId, callback);
            }
            else
            {
                throw new ArgumentException(NotInsideRoomMessage, nameof(playerId));
            }
        }

        private void HandleChannelEnded(string roomCode, string playerId, Contracts.IChatCallback callback)
        {
            _callbacks.RemoveIfCurrent(roomCode, playerId, callback);
        }
    }
}
