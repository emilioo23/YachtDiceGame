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
        private readonly ConcurrentDictionary<string, List<Contracts.IChatCallback>> _chatRooms = new ConcurrentDictionary<string, List<Contracts.IChatCallback>>();
        private readonly ConcurrentDictionary<string, List<Contracts.IGameCallback>> _gameRooms = new ConcurrentDictionary<string, List<Contracts.IGameCallback>>();
        private readonly ConcurrentDictionary<string, List<Contracts.IRoomCallback>> _roomManagers = new ConcurrentDictionary<string, List<Contracts.IRoomCallback>>();
        private readonly AppLogger _logger;

        /// <summary>
        /// Initializes a new instance of the GameService.
        /// </summary>
        public GameService()
        {
            _logger = new AppLogger(typeof(GameService));
        }

        public void RollDice(string roomId, string username)
        {
            Random rng = new Random();
            int[] diceValues = { rng.Next(1, 7), rng.Next(1, 7), rng.Next(1, 7), rng.Next(1, 7), rng.Next(1, 7) };

            RegisterChannel(roomId, OperationContext.Current.GetCallbackChannel<Contracts.IGameCallback>(), _gameRooms);

            if (_gameRooms.TryGetValue(roomId, out var callbacks))
            {
                NotifyClients(callbacks, client => client.OnDiceRolled(diceValues));
            }
        }

        public void SelectScoreCategory(string roomId, string username, int categoryId)
        {
            if (_gameRooms.TryGetValue(roomId, out var callbacks))
            {
                NotifyClients(callbacks, client => client.OnTurnChanged(new Contracts.TurnDetails()));
            }
        }

        public void JoinRoom(Contracts.PlayerProfile player, string roomId)
        {
            var roomCallback = OperationContext.Current.GetCallbackChannel<Contracts.IRoomCallback>();
            RegisterChannel(roomId, roomCallback, _roomManagers);

            if (_roomManagers.TryGetValue(roomId, out var callbacks))
            {
                NotifyClients(callbacks, client => client.OnPlayerChanged(player, Contracts.RoomEventType.PlayerJoined));
            }
        }

        public void LeaveRoom(string username, string roomId)
        {
            var roomCallback = OperationContext.Current.GetCallbackChannel<Contracts.IRoomCallback>();

            if (_roomManagers.TryGetValue(roomId, out var callbacks))
            {
                lock (callbacks)
                {
                    callbacks.Remove(roomCallback);
                }

                var leavingPlayer = new Contracts.PlayerProfile { Username = username };
                NotifyClients(callbacks, client => client.OnPlayerChanged(leavingPlayer, Contracts.RoomEventType.PlayerLeft));
            }
        }

        public void SendMessage(Contracts.ChatMessage message, string roomId)
        {
            var chatCallback = OperationContext.Current.GetCallbackChannel<Contracts.IChatCallback>();
            RegisterChannel(roomId, chatCallback, _chatRooms);

            if (_chatRooms.TryGetValue(roomId, out var callbacks))
            {
                NotifyClients(callbacks, client => client.OnMessageReceived(message));
            }
        }

        /// <summary>
        /// Registers the client's communication channel in the specified room, avoiding duplicates.
        /// </summary>
        /// <typeparam name="T">The type of the callback contract.</typeparam>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="callback">The client's callback channel.</param>
        /// <param name="roomDictionary">The concurrent dictionary where callbacks are stored by room.</param>
        private void RegisterChannel<T>(string roomId, T callback, ConcurrentDictionary<string, List<T>> roomDictionary) where T : class
        {
            roomDictionary.AddOrUpdate(roomId,
                new List<T> { callback },
                (key, list) =>
                {
                    lock (list)
                    {
                        if (!list.Contains(callback)) list.Add(callback);
                    }
                    return list;
                });
        }

        /// <summary>
        /// Notifies all registered clients in a room and removes closed or inactive channels from the list.
        /// </summary>
        /// <typeparam name="T">The type of the callback contract.</typeparam>
        /// <param name="callbacks">The list of channels registered in the room.</param>
        /// <param name="action">The action or method to invoke on each client.</param>
        private void NotifyClients<T>(List<T> callbacks, Action<T> action) where T : class
        {
            lock (callbacks)
            {
                for (int i = callbacks.Count - 1; i >= 0; i--)
                {
                    var callback = callbacks[i];
                    var communicationObject = (ICommunicationObject)callback;

                    if (communicationObject.State == CommunicationState.Opened)
                    {
                        try
                        {
                            action(callback);
                        }
                        catch (CommunicationException ex)
                        {
                            _logger.Warning($"[CALLBACK DEMO] Attempted to push to a disconnected client. Exception: {ex.GetType().Name} - {ex.Message}");
                            callbacks.RemoveAt(i);
                        }
                        catch (TimeoutException)
                        {
                            callbacks.RemoveAt(i);
                        }
                    }
                    else
                    {
                        callbacks.RemoveAt(i);
                    }
                }
            }
        }
    }
}