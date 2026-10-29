using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using YachtDice.Server.Logic.Logging;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;

    /// <summary>
    /// Implements the game, room, and chat management operations.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class GameService : ServiceBase, Contracts.IGameManager, Contracts.IRoomManager, Contracts.IChatManager
    {
        private const int DiceCount = 5;
        private const int MinDieValue = 1;
        private const int MaxDieValue = 6;
        private const string DisconnectedClientMessage = "A notification could not be delivered to a client.";

        private readonly ConcurrentDictionary<string, List<Contracts.IChatCallback>> _chatRooms =
            new ConcurrentDictionary<string, List<Contracts.IChatCallback>>();

        private readonly ConcurrentDictionary<string, List<Contracts.IGameCallback>> _gameRooms =
            new ConcurrentDictionary<string, List<Contracts.IGameCallback>>();

        private readonly ConcurrentDictionary<string, List<Contracts.IRoomCallback>> _roomManagers =
            new ConcurrentDictionary<string, List<Contracts.IRoomCallback>>();

        private readonly Random _random = new Random();
        private readonly AppLogger _logger;

        /// <summary>
        /// Initializes a new instance of the GameService.
        /// </summary>
        public GameService()
        {
            _logger = new AppLogger(typeof(GameService));
        }

        /// <summary>
        /// Rolls the five dice and notifies every client in the room.
        /// </summary>
        /// <param name="roomId">Identifier of the room.</param>
        /// <param name="username">Username of the player who rolls.</param>
        public void RollDice(string roomId, string username)
        {
            int[] diceValues = CreateDiceValues();

            RegisterChannel(roomId, OperationContext.Current.GetCallbackChannel<Contracts.IGameCallback>(), _gameRooms);

            if (_gameRooms.TryGetValue(roomId, out var callbacks))
            {
                NotifyClients(callbacks, client => client.DiceRolled(diceValues));
            }
        }

        /// <summary>
        /// Scores a category and notifies every client in the room that the turn changed.
        /// </summary>
        /// <param name="roomId">Identifier of the room.</param>
        /// <param name="username">Username of the player who scores.</param>
        /// <param name="categoryId">Identifier of the selected category.</param>
        public void SelectScoreCategory(string roomId, string username, int categoryId)
        {
            if (_gameRooms.TryGetValue(roomId, out var callbacks))
            {
                NotifyClients(callbacks, client => client.TurnChanged(new Contracts.TurnDetails()));
            }
        }

        /// <summary>
        /// Adds a player to a room and notifies every client in it.
        /// </summary>
        /// <param name="player">Profile of the player who joins.</param>
        /// <param name="roomId">Identifier of the room.</param>
        public void JoinRoom(Contracts.PlayerProfile player, string roomId)
        {
            var roomCallback = OperationContext.Current.GetCallbackChannel<Contracts.IRoomCallback>();
            RegisterChannel(roomId, roomCallback, _roomManagers);

            if (_roomManagers.TryGetValue(roomId, out var callbacks))
            {
                NotifyClients(callbacks, client => client.PlayerChanged(player, Contracts.RoomEventType.PlayerJoined));
            }
        }

        /// <summary>
        /// Removes a player from a room and notifies the remaining clients.
        /// </summary>
        /// <param name="username">Username of the player who leaves.</param>
        /// <param name="roomId">Identifier of the room.</param>
        public void LeaveRoom(string username, string roomId)
        {
            var roomCallback = OperationContext.Current.GetCallbackChannel<Contracts.IRoomCallback>();

            if (_roomManagers.TryGetValue(roomId, out var callbacks))
            {
                RemoveChannel(callbacks, roomCallback);

                var leavingPlayer = new Contracts.PlayerProfile { Username = username };
                NotifyClients(callbacks, client => client.PlayerChanged(leavingPlayer, Contracts.RoomEventType.PlayerLeft));
            }
        }

        /// <summary>
        /// Sends a chat message to every client in the room.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="roomId">Identifier of the room.</param>
        public void SendMessage(Contracts.ChatMessage message, string roomId)
        {
            var chatCallback = OperationContext.Current.GetCallbackChannel<Contracts.IChatCallback>();
            RegisterChannel(roomId, chatCallback, _chatRooms);

            if (_chatRooms.TryGetValue(roomId, out var callbacks))
            {
                NotifyClients(callbacks, client => client.MessageReceived(message));
            }
        }

        private int[] CreateDiceValues()
        {
            var diceValues = new int[DiceCount];

            lock (_random)
            {
                for (int diceIndex = 0; diceIndex < DiceCount; diceIndex++)
                {
                    diceValues[diceIndex] = _random.Next(MinDieValue, MaxDieValue + 1);
                }
            }

            return diceValues;
        }

        private void RegisterChannel<T>(
            string roomId,
            T callback,
            ConcurrentDictionary<string, List<T>> roomDictionary) where T : class
        {
            roomDictionary.AddOrUpdate(
                roomId,
                new List<T> { callback },
                (key, callbacks) => AddIfMissing(callbacks, callback));
        }

        private static List<T> AddIfMissing<T>(List<T> callbacks, T callback)
        {
            lock (callbacks)
            {
                if (!callbacks.Contains(callback))
                {
                    callbacks.Add(callback);
                }
            }

            return callbacks;
        }

        private static void RemoveChannel<T>(List<T> callbacks, T callback)
        {
            lock (callbacks)
            {
                callbacks.Remove(callback);
            }
        }

        private void NotifyClients<T>(List<T> callbacks, Action<T> action) where T : class
        {
            lock (callbacks)
            {
                for (int callbackIndex = callbacks.Count - 1; callbackIndex >= 0; callbackIndex--)
                {
                    if (!TryNotify(callbacks[callbackIndex], action))
                    {
                        callbacks.RemoveAt(callbackIndex);
                    }
                }
            }
        }

        private bool TryNotify<T>(T callback, Action<T> action) where T : class
        {
            bool isDelivered = false;
            var communicationObject = (ICommunicationObject)callback;

            if (communicationObject.State == CommunicationState.Opened)
            {
                try
                {
                    action(callback);
                    isDelivered = true;
                }
                catch (CommunicationException ex)
                {
                    _logger.Warning(DisconnectedClientMessage + " " + ex.GetType().Name);
                }
                catch (TimeoutException ex)
                {
                    _logger.Warning(DisconnectedClientMessage + " " + ex.GetType().Name);
                }
            }

            return isDelivered;
        }
    }
}