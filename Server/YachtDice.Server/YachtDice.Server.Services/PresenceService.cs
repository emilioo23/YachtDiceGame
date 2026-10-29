using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;
    using LogicLogging = YachtDice.Server.Logic.Logging;

    /// <summary>
    /// Keeps track of the players who are online and warns the others when someone comes or goes.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class PresenceService : Contracts.IPresenceService
    {
        private const string DeliveryFailedMessage = "A presence notification could not be delivered: ";
        private const string ConnectedLogMessage = "A player is now online.";
        private const string DisconnectedLogMessage = "A player is now offline.";

        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, OnlinePlayer> _players =
            new Dictionary<string, OnlinePlayer>(StringComparer.OrdinalIgnoreCase);
        private readonly LogicLogging.AppLogger _logger = new LogicLogging.AppLogger(typeof(PresenceService));

        /// <summary>
        /// Marks the player as online and warns the other players.
        /// </summary>
        /// <param name="displayName">Name of the player who enters the game.</param>
        /// <returns>The names of all the players who are online, including the caller.</returns>
        public string[] Connect(string displayName)
        {
            string[] onlinePlayers = new string[0];

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                OnlinePlayer player = CreateCurrentPlayer();

                lock (_syncRoot)
                {
                    _players[displayName] = player;
                    onlinePlayers = _players.Keys.ToArray();
                }

                WatchChannel(displayName, player.SessionId);
                _logger.Info(ConnectedLogMessage);
                Broadcast(displayName, Contracts.PresenceEventType.PlayerConnected);
            }

            return onlinePlayers;
        }

        /// <summary>
        /// Marks the player as offline and warns the other players.
        /// </summary>
        /// <param name="displayName">Name of the player who leaves the game.</param>
        public void Disconnect(string displayName)
        {
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                Remove(displayName, OperationContext.Current.SessionId);
            }
        }

        private static OnlinePlayer CreateCurrentPlayer()
        {
            return new OnlinePlayer(
                OperationContext.Current.SessionId,
                OperationContext.Current.GetCallbackChannel<Contracts.IPresenceCallback>());
        }

        private void WatchChannel(string displayName, string sessionId)
        {
            ICommunicationObject channel = OperationContext.Current.Channel;

            channel.Closed += (sender, args) => Remove(displayName, sessionId);
            channel.Faulted += (sender, args) => Remove(displayName, sessionId);
        }

        private void Remove(string displayName, string sessionId)
        {
            bool wasRemoved = false;
            OnlinePlayer player;

            lock (_syncRoot)
            {
                if (_players.TryGetValue(displayName, out player) && player.SessionId == sessionId)
                {
                    _players.Remove(displayName);
                    wasRemoved = true;
                }
            }

            if (wasRemoved)
            {
                _logger.Info(DisconnectedLogMessage);
                Broadcast(displayName, Contracts.PresenceEventType.PlayerDisconnected);
            }
        }

        private void Broadcast(string displayName, Contracts.PresenceEventType eventType)
        {
            List<KeyValuePair<string, OnlinePlayer>> recipients;

            lock (_syncRoot)
            {
                recipients = _players
                    .Where(entry => !string.Equals(entry.Key, displayName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            foreach (KeyValuePair<string, OnlinePlayer> recipient in recipients)
            {
                if (!TryNotify(recipient.Value.Callback, displayName, eventType))
                {
                    Remove(recipient.Key, recipient.Value.SessionId);
                }
            }
        }

        private bool TryNotify(
            Contracts.IPresenceCallback callback,
            string displayName,
            Contracts.PresenceEventType eventType)
        {
            bool isDelivered = false;

            if (((ICommunicationObject)callback).State == CommunicationState.Opened)
            {
                try
                {
                    callback.PresenceChanged(displayName, eventType);
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

        /// <summary>
        /// A player who is online, with the session and the callback channel used to reach the player.
        /// </summary>
        private sealed class OnlinePlayer
        {
            /// <summary>
            /// Initializes the player with its session and callback channel.
            /// </summary>
            /// <param name="sessionId">Identifier of the session of the player.</param>
            /// <param name="callback">Callback channel of the player.</param>
            public OnlinePlayer(string sessionId, Contracts.IPresenceCallback callback)
            {
                SessionId = sessionId;
                Callback = callback;
            }

            /// <summary>
            /// Gets the identifier of the session of the player.
            /// </summary>
            public string SessionId { get; }

            /// <summary>
            /// Gets the callback channel of the player.
            /// </summary>
            public Contracts.IPresenceCallback Callback { get; }
        }
    }
}
