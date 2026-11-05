using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using LogicLogging = YachtDice.Server.Logic.Logging;

    /// <summary>
    /// Keeps the callback channel of each player subscribed to a room and notifies them. Callers come from
    /// several threads (the WCF operations and the events of the channels), so every access to the
    /// dictionaries takes the lock. The lock is released before any client is notified, because a slow
    /// client must not stop the others.
    /// </summary>
    /// <typeparam name="TCallback">Callback contract the clients implement.</typeparam>
    internal class CallbackRegistry<TCallback> where TCallback : class
    {
        private const string DeliveryFailedMessage = "A notification could not be delivered to a client: ";

        private readonly object _syncRoot = new object();

        private readonly Dictionary<string, Dictionary<string, TCallback>> _callbacksByRoom =
            new Dictionary<string, Dictionary<string, TCallback>>(StringComparer.OrdinalIgnoreCase);

        private readonly LogicLogging.AppLogger _logger =
            new LogicLogging.AppLogger(typeof(CallbackRegistry<TCallback>));

        /// <summary>
        /// Subscribes the callback channel of a player to a room. If the player was already subscribed, the
        /// new channel replaces the old one.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="callback">Callback channel of the player.</param>
        public void Register(string roomCode, string playerName, TCallback callback)
        {
            lock (_syncRoot)
            {
                if (!_callbacksByRoom.TryGetValue(roomCode, out Dictionary<string, TCallback> callbacks))
                {
                    callbacks = new Dictionary<string, TCallback>(StringComparer.OrdinalIgnoreCase);
                    _callbacksByRoom[roomCode] = callbacks;
                }

                callbacks[playerName] = callback;
            }
        }

        /// <summary>
        /// Unsubscribes a player from a room, whatever channel the player used.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerName">Name of the player.</param>
        public void Remove(string roomCode, string playerName)
        {
            lock (_syncRoot)
            {
                DeleteCallback(roomCode, playerName);
            }
        }

        /// <summary>
        /// Unsubscribes a player only if the channel that is registered is the given one. It avoids that a
        /// channel that is already closed removes the new channel of the same player.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="callback">Callback channel that should be the registered one.</param>
        /// <returns>True if the player was unsubscribed.</returns>
        public bool RemoveIfCurrent(string roomCode, string playerName, TCallback callback)
        {
            var isRemoved = false;

            lock (_syncRoot)
            {
                if (TryGetCallback(roomCode, playerName, out TCallback current) && ReferenceEquals(current, callback))
                {
                    DeleteCallback(roomCode, playerName);
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
            foreach (KeyValuePair<string, TCallback> recipient in GetRecipients(roomCode))
            {
                if (!TryNotify(recipient.Value, notification))
                {
                    RemoveIfCurrent(roomCode, recipient.Key, recipient.Value);
                }
            }
        }

        private List<KeyValuePair<string, TCallback>> GetRecipients(string roomCode)
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

        // The caller must hold _syncRoot.
        private bool TryGetCallback(string roomCode, string playerName, out TCallback callback)
        {
            callback = null;

            return _callbacksByRoom.TryGetValue(roomCode, out Dictionary<string, TCallback> callbacks)
                && callbacks.TryGetValue(playerName, out callback);
        }

        // The caller must hold _syncRoot.
        private void DeleteCallback(string roomCode, string playerName)
        {
            if (_callbacksByRoom.TryGetValue(roomCode, out Dictionary<string, TCallback> callbacks))
            {
                callbacks.Remove(playerName);

                if (callbacks.Count == 0)
                {
                    _callbacksByRoom.Remove(roomCode);
                }
            }
        }

        private bool TryNotify(TCallback callback, Action<TCallback> notification)
        {
            var isDelivered = false;
            var communicationObject = (ICommunicationObject)callback;

            if (communicationObject.State == CommunicationState.Opened)
            {
                try
                {
                    notification(callback);
                    isDelivered = true;
                }
                catch (CommunicationException ex)
                {
                    _logger.Warning(DeliveryFailedMessage + ex.GetType().Name);
                }
                catch (TimeoutException ex)
                {
                    _logger.Warning(DeliveryFailedMessage + ex.GetType().Name);
                }
            }

            return isDelivered;
        }
    }
}
