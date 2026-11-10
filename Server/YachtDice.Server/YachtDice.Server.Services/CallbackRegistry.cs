using System;
using System.Collections.Generic;

namespace YachtDice.Server.Services
{
    using LogicLogging = YachtDice.Server.Logic.Logging;

    /// <summary>
    /// Keeps the callback channel of each player subscribed to a room and notifies them. The events of the
    /// channels run outside the calls that WCF serializes, so every access to the dictionaries takes the lock.
    /// The lock is released before any client is notified, because a slow client must not stop the others.
    /// A concurrent dictionary is not enough because the data has two levels and an empty room is deleted.
    /// </summary>
    /// <typeparam name="TCallback">Callback contract the clients implement.</typeparam>
    internal class CallbackRegistry<TCallback> where TCallback : class
    {
        private readonly object _syncRoot = new object();

        private readonly Dictionary<string, Dictionary<string, TCallback>> _callbacksByRoom =
            new Dictionary<string, Dictionary<string, TCallback>>(StringComparer.OrdinalIgnoreCase);

        private readonly LogicLogging.AppLogger _logger;

        /// <summary>
        /// Initializes an empty registry.
        /// </summary>
        public CallbackRegistry()
        {
            _logger = new LogicLogging.AppLogger(GetType());
        }

        /// <summary>
        /// Subscribes the callback channel of a player to a room. If the player was already subscribed, the
        /// new channel replaces the old one.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="callback">Callback channel of the player.</param>
        public void Register(string roomCode, string playerId, TCallback callback)
        {
            lock (_syncRoot)
            {
                if (!_callbacksByRoom.TryGetValue(roomCode, out Dictionary<string, TCallback> callbacks))
                {
                    callbacks = new Dictionary<string, TCallback>(StringComparer.Ordinal);
                    _callbacksByRoom[roomCode] = callbacks;
                }

                callbacks[playerId] = callback;
            }
        }

        /// <summary>
        /// Unsubscribes a player from a room, whatever channel the player used.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Identifier of the player.</param>
        public void Remove(string roomCode, string playerId)
        {
            lock (_syncRoot)
            {
                DeleteCallback(roomCode, playerId);
            }
        }

        /// <summary>
        /// Unsubscribes a player only if the channel that is registered is the given one. It avoids that a
        /// channel that is already closed removes the new channel of the same player.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="callback">Callback channel that should be the registered one.</param>
        /// <returns>True if the player was unsubscribed.</returns>
        public bool RemoveIfCurrent(string roomCode, string playerId, TCallback callback)
        {
            var isRemoved = false;

            lock (_syncRoot)
            {
                if (TryGetCallback(roomCode, playerId, out TCallback current) && ReferenceEquals(current, callback))
                {
                    DeleteCallback(roomCode, playerId);
                    isRemoved = true;
                }
            }

            return isRemoved;
        }

        /// <summary>
        /// Sends a notification to every player subscribed to the room. The players that cannot be reached are
        /// unsubscribed.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="notification">Call to make on each callback channel.</param>
        public void Notify(string roomCode, Action<TCallback> notification)
        {
            foreach (KeyValuePair<string, TCallback> recipient in CopyRecipients(roomCode))
            {
                if (!CallbackDelivery.TryDeliver(recipient.Value, notification, _logger))
                {
                    RemoveIfCurrent(roomCode, recipient.Key, recipient.Value);
                }
            }
        }

        /// <summary>
        /// Sends a notification only to one player subscribed to the room. If the player cannot be reached, it
        /// is unsubscribed.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Identifier of the player to notify.</param>
        /// <param name="notification">Call to make on the callback channel.</param>
        public void NotifyPlayer(string roomCode, string playerId, Action<TCallback> notification)
        {
            TCallback callback = null;
            var isSubscribed = false;

            lock (_syncRoot)
            {
                isSubscribed = TryGetCallback(roomCode, playerId, out callback);
            }

            if (isSubscribed && !CallbackDelivery.TryDeliver(callback, notification, _logger))
            {
                RemoveIfCurrent(roomCode, playerId, callback);
            }
        }

        private List<KeyValuePair<string, TCallback>> CopyRecipients(string roomCode)
        {
            var recipients = new List<KeyValuePair<string, TCallback>>();

            lock (_syncRoot)
            {
                if (_callbacksByRoom.TryGetValue(roomCode, out Dictionary<string, TCallback> callbacks))
                {
                    recipients.AddRange(callbacks);
                }
            }

            return recipients;
        }

        private bool TryGetCallback(string roomCode, string playerId, out TCallback callback)
        {
            callback = null;

            return _callbacksByRoom.TryGetValue(roomCode, out Dictionary<string, TCallback> callbacks)
                && callbacks.TryGetValue(playerId, out callback);
        }

        private void DeleteCallback(string roomCode, string playerId)
        {
            if (_callbacksByRoom.TryGetValue(roomCode, out Dictionary<string, TCallback> callbacks))
            {
                callbacks.Remove(playerId);

                if (callbacks.Count == 0)
                {
                    _callbacksByRoom.Remove(roomCode);
                }
            }
        }
    }
}
