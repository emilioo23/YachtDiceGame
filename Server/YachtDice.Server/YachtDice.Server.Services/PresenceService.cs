using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Presence;
    using LogicLogging = YachtDice.Server.Logic.Logging;

    /// <summary>
    /// Keeps track of the players who are online and warns the others when someone comes or goes. The list is
    /// also used by the friend service and by the events of the channels, which run outside the calls that WCF
    /// serializes on this instance, so it is a concurrent collection.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class PresenceService : Contracts.IPresenceService, IPlayerNotifier
    {
        private const string ConnectedLogMessage = "A player is now online.";
        private const string DisconnectedLogMessage = "A player is now offline.";

        private readonly ConcurrentDictionary<string, OnlinePlayer> _players =
            new ConcurrentDictionary<string, OnlinePlayer>(StringComparer.OrdinalIgnoreCase);

        private readonly LogicLogging.AppLogger _logger;

        /// <summary>
        /// Initializes a new instance of the PresenceService class.
        /// </summary>
        public PresenceService()
        {
            _logger = new LogicLogging.AppLogger(GetType());
        }

        /// <summary>
        /// Marks the player as online and warns the other players.
        /// </summary>
        /// <param name="displayName">Name of the player who enters the game.</param>
        /// <returns>The names of all the players who are online, including the caller.</returns>
        public string[] Connect(string displayName)
        {
            string[] onlinePlayers = Array.Empty<string>();

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                OnlinePlayer player = CreateCurrentPlayer();

                _players[displayName] = player;
                onlinePlayers = _players.Keys.ToArray();

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

        /// <summary>
        /// Sends the event to the player, if the player is online.
        /// </summary>
        /// <param name="recipientName">Name of the player who receives the event.</param>
        /// <param name="subjectName">Name of the player the event is about.</param>
        /// <param name="eventType">What happened with the subject.</param>
        public void SendEvent(string recipientName, string subjectName, Contracts.PresenceEventType eventType)
        {
            if (_players.TryGetValue(recipientName, out OnlinePlayer recipient))
            {
                bool isDelivered = CallbackDelivery.TryDeliver(
                    recipient.Callback,
                    callback => callback.PresenceChanged(subjectName, eventType),
                    _logger);

                if (!isDelivered)
                {
                    Remove(recipientName, recipient.SessionId);
                }
            }
        }

        /// <summary>
        /// Sends an invitation to a room to the player, if the player is online.
        /// </summary>
        /// <param name="recipientName">Name of the player who receives the invitation.</param>
        /// <param name="invitation">Who invites and the code of the room.</param>
        /// <returns>True if the invitation reached the player.</returns>
        public bool SendInvitation(string recipientName, Contracts.RoomInvitation invitation)
        {
            var isDelivered = false;

            if (_players.TryGetValue(recipientName, out OnlinePlayer recipient))
            {
                isDelivered = CallbackDelivery.TryDeliver(
                    recipient.Callback,
                    callback => callback.InvitationReceived(invitation),
                    _logger);

                if (!isDelivered)
                {
                    Remove(recipientName, recipient.SessionId);
                }
            }

            return isDelivered;
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

            channel.Closed += (sender, e) => Remove(displayName, sessionId);
            channel.Faulted += (sender, e) => Remove(displayName, sessionId);
        }

        private void Remove(string displayName, string sessionId)
        {
            var isRemoved = false;

            if (_players.TryGetValue(displayName, out OnlinePlayer player))
            {
                bool isSameSession = player.SessionId == sessionId;

                if (isSameSession)
                {
                    isRemoved = RemoveEntry(displayName, player);
                }
            }

            if (isRemoved)
            {
                _logger.Info(DisconnectedLogMessage);
                Broadcast(displayName, Contracts.PresenceEventType.PlayerDisconnected);
            }
        }

        private bool RemoveEntry(string displayName, OnlinePlayer player)
        {
            var entry = new KeyValuePair<string, OnlinePlayer>(displayName, player);

            return ((ICollection<KeyValuePair<string, OnlinePlayer>>)_players).Remove(entry);
        }

        private void Broadcast(string displayName, Contracts.PresenceEventType eventType)
        {
            foreach (string recipientName in _players.Keys)
            {
                bool isOtherPlayer = !string.Equals(recipientName, displayName, StringComparison.OrdinalIgnoreCase);

                if (isOtherPlayer)
                {
                    SendEvent(recipientName, displayName, eventType);
                }
            }
        }
    }
}
