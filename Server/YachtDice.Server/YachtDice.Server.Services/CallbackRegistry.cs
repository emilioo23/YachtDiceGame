using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using LogicLogging = YachtDice.Server.Logic.Logging;

    /// <summary>
    /// Keeps the callback channels of the clients connected to each room and notifies them.
    /// </summary>
    /// <typeparam name="TCallback">Callback contract the clients implement.</typeparam>
    internal class CallbackRegistry<TCallback> where TCallback : class
    {
        private const string DeliveryFailedMessage = "A notification could not be delivered to a client: ";

        private readonly ConcurrentDictionary<string, List<TCallback>> _callbacksByRoom =
            new ConcurrentDictionary<string, List<TCallback>>();

        private readonly LogicLogging.AppLogger _logger =
            new LogicLogging.AppLogger(typeof(CallbackRegistry<TCallback>));

        /// <summary>
        /// Subscribes a client channel to a room. If it was already subscribed, nothing changes.
        /// </summary>
        /// <param name="roomId">Identifier of the room.</param>
        /// <param name="callback">Callback channel of the client.</param>
        public void Register(string roomId, TCallback callback)
        {
            List<TCallback> callbacks = _callbacksByRoom.GetOrAdd(roomId, key => new List<TCallback>());

            lock (callbacks)
            {
                if (!callbacks.Contains(callback))
                {
                    callbacks.Add(callback);
                }
            }
        }

        /// <summary>
        /// Removes a client channel from a room.
        /// </summary>
        /// <param name="roomId">Identifier of the room.</param>
        /// <param name="callback">Callback channel of the client.</param>
        public void Remove(string roomId, TCallback callback)
        {
            List<TCallback> callbacks;

            if (_callbacksByRoom.TryGetValue(roomId, out callbacks))
            {
                lock (callbacks)
                {
                    callbacks.Remove(callback);
                }
            }
        }

        /// <summary>
        /// Sends a notification to every client in a room. Clients that cannot be reached are removed.
        /// </summary>
        /// <param name="roomId">Identifier of the room.</param>
        /// <param name="notification">Call to make on each client callback.</param>
        public void Notify(string roomId, Action<TCallback> notification)
        {
            List<TCallback> callbacks;

            if (_callbacksByRoom.TryGetValue(roomId, out callbacks))
            {
                lock (callbacks)
                {
                    callbacks.RemoveAll(callback => !TryNotify(callback, notification));
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
