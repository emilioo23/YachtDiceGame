using System;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Chat;
    using LogicLogging = YachtDice.Server.Logic.Logging;
    using LogicModeration = YachtDice.Server.Logic.Moderation;
    using LogicRooms = YachtDice.Server.Logic.Rooms;
    using RoomContracts = YachtDice.Server.Contracts.Rooms;

    /// <summary>
    /// Delivers the chat messages to every player subscribed to a room. Only the players who are inside the
    /// room can write in its chat.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ChatService : ServiceBase, Contracts.IChatService
    {
        private const int MaxMessageLength = 200;
        private const int FirstCharacterIndex = 0;
        private const string IgnoredMessageLogMessage =
            "A chat message was ignored because it is empty or its author is not inside the room.";

        private const string NotInsideRoomMessage = "A player who is not inside the room cannot join its chat.";

        private const string NullWordFilterMessage = "The filter of offensive words cannot be null.";

        private readonly LogicRooms.IRoomManager _roomManager;
        private readonly LogicModeration.OffensiveWordFilter _wordFilter;

        private readonly CallbackRegistry<Contracts.IChatCallback> _callbacks =
            new CallbackRegistry<Contracts.IChatCallback>();

        private readonly LogicLogging.AppLogger _logger;

        /// <summary>
        /// Initializes a new instance of the ChatService class.
        /// </summary>
        /// <param name="roomManager">The rules and the state of the rooms. It is shared with the room service.</param>
        public ChatService(LogicRooms.IRoomManager roomManager, LogicModeration.OffensiveWordFilter wordFilter)
        {
            _roomManager = roomManager
                ?? throw new ArgumentNullException(nameof(roomManager));
            _wordFilter = wordFilter ?? throw new ArgumentNullException(nameof(wordFilter), NullWordFilterMessage);
            _logger = new LogicLogging.AppLogger(GetType());
        }

        /// <summary>
        /// Subscribes the player to the messages of the room. The player must already be inside the room.
        /// </summary>
        /// <param name="member">Room and identifier of the player who joins the chat.</param>
        public void JoinChat(RoomContracts.RoomMember member)
        {
            LogicRooms.RoomMember joining = Mapping.RoomMapper.ToLogic(member);

            Execute(member, () => ProcessJoin(joining));
        }

        /// <summary>
        /// Sends a chat message to every player subscribed to the room, including the sender. The author is
        /// the player that has the identifier, so a player cannot write in the name of another one.
        /// </summary>
        /// <param name="author">Room and identifier of the player who writes the message.</param>
        /// <param name="content">Text of the message.</param>
        public void SendChatMessage(RoomContracts.RoomMember author, string content)
        {
            LogicRooms.RoomMember member = Mapping.RoomMapper.ToLogic(author);
            var hasContent = !string.IsNullOrWhiteSpace(content);

            if (hasContent && _roomManager.TryGetPlayer(member, out LogicRooms.RoomPlayer writer))
            {
                Contracts.ChatMessage delivered = CreateDeliveredMessage(writer, content);

                _callbacks.Notify(member.RoomCode, callback => callback.MessageReceived(delivered));
            }
            else
            {
                _logger.Warning(IgnoredMessageLogMessage);
            }
        }

        /// <summary>
        /// Stops sending the messages of the room to the player.
        /// </summary>
        /// <param name="member">Room and identifier of the player who leaves the chat.</param>
        public void LeaveChat(RoomContracts.RoomMember member)
        {
            LogicRooms.RoomMember leaving = Mapping.RoomMapper.ToLogic(member);
            bool hasPlayer = !string.IsNullOrWhiteSpace(leaving.PlayerId);

            if (hasPlayer && !string.IsNullOrWhiteSpace(leaving.RoomCode))
            {
                _callbacks.Remove(leaving);
            }
        }

        private Contracts.ChatMessage CreateDeliveredMessage(LogicRooms.RoomPlayer author, string content)
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
                AvatarId = author.AvatarId,
                Content = LogicModeration.LinkCensor.Censor(_wordFilter.Censor(text)),
                Timestamp = DateTime.UtcNow
            };

            return delivered;
        }

        private void ProcessJoin(LogicRooms.RoomMember member)
        {
            if (_roomManager.TryGetPlayer(member, out _))
            {
                Contracts.IChatCallback callback =
                    OperationContext.Current.GetCallbackChannel<Contracts.IChatCallback>();
                ICommunicationObject channel = OperationContext.Current.Channel;

                _callbacks.Register(member, callback);
                channel.Closed += (sender, e) => HandleChannelEnded(member, callback);
                channel.Faulted += (sender, e) => HandleChannelEnded(member, callback);
            }
            else
            {
                throw new ArgumentException(NotInsideRoomMessage, nameof(member));
            }
        }

        private void HandleChannelEnded(LogicRooms.RoomMember member, Contracts.IChatCallback callback)
        {
            _callbacks.RemoveIfCurrent(member, callback);
        }
    }
}
