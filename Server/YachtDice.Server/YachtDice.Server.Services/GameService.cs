using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using YachtDice.Server.Services.Contracts;

namespace YachtDice.Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class GameService : IGameManager, IRoomManager, IChatManager
    {
        private readonly ConcurrentDictionary<string, List<IChatCallback>> _chatRooms = new ConcurrentDictionary<string, List<IChatCallback>>();
        private readonly ConcurrentDictionary<string, List<IGameCallback>> _gameRooms = new ConcurrentDictionary<string, List<IGameCallback>>();
        private readonly ConcurrentDictionary<string, List<IRoomManagerCallback>> _roomManagers = new ConcurrentDictionary<string, List<IRoomManagerCallback>>();

        public void RollDice(string roomId, string username)
        {
            Random rng = new Random();
            int[] diceValues = { rng.Next(1, 7), rng.Next(1, 7), rng.Next(1, 7), rng.Next(1, 7), rng.Next(1, 7) };

            RegisterChannel(roomId, OperationContext.Current.GetCallbackChannel<IGameCallback>(), _gameRooms);

            if (_gameRooms.TryGetValue(roomId, out var callbacks))
            {
                NotifyClients(callbacks, client => client.OnDiceRolled(diceValues));
            }
        }

        public void SelectScoreCategory(string roomId, string username, int categoryId)
        {
            if (_gameRooms.TryGetValue(roomId, out var callbacks))
            {
                NotifyClients(callbacks, client => client.OnTurnChanged(new TurnDetails()));
            }
        }

        public void JoinRoom(PlayerProfile player, string roomId)
        {
            var roomCallback = OperationContext.Current.GetCallbackChannel<IRoomManagerCallback>();
            RegisterChannel(roomId, roomCallback, _roomManagers);

            if (_roomManagers.TryGetValue(roomId, out var callbacks))
            {
                NotifyClients(callbacks, client => client.OnPlayerJoined(player));
            }
        }

        public void LeaveRoom(string username, string roomId)
        {
            var roomCallback = OperationContext.Current.GetCallbackChannel<IRoomManagerCallback>();

            if (_roomManagers.TryGetValue(roomId, out var callbacks))
            {
                lock (callbacks)
                {
                    callbacks.Remove(roomCallback);
                }

                NotifyClients(callbacks, client => client.OnPlayerLeft(username));
            }
        }

        public void SendMessage(ChatMessage message, string roomId)
        {
            var chatCallback = OperationContext.Current.GetCallbackChannel<IChatCallback>();
            RegisterChannel(roomId, chatCallback, _chatRooms);

            if (_chatRooms.TryGetValue(roomId, out var callbacks))
            {
                NotifyClients(callbacks, client => client.OnMessageReceived(message));
            }
        }

        /// <summary>
        /// Registra el canal de comunicación del cliente en la sala especificada evitando duplicados.
        /// </summary>
        /// <typeparam name="T">El tipo de contrato de callback.</typeparam>
        /// <param name="roomId">El identificador de la sala.</param>
        /// <param name="callback">El canal de callback del cliente.</param>
        /// <param name="roomDictionary">El diccionario concurrente donde se almacenan los callbacks por sala.</param>
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
        /// Notifica a todos los clientes registrados en una sala y elimina de la lista aquellos canales cerrados o inactivos.
        /// </summary>
        /// <typeparam name="T">El tipo de contrato de callback.</typeparam>
        /// <param name="callbacks">La lista de canales registrados en la sala.</param>
        /// <param name="action">La acción o método a invocar en cada cliente.</param>
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
                        catch (CommunicationException)
                        {
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