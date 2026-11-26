using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Presence;
    using LogicFriends = YachtDice.Server.Logic.Friends;
    using LogicLogging = YachtDice.Server.Logic.Logging;
    using LogicSecurity = YachtDice.Server.Logic.Security;

    /// <summary>
    /// Keeps track of the players who are online and warns the others when someone comes or goes. The list is
    /// also used by the friend service and by the events of the channels, which run outside the calls that WCF
    /// serializes on this instance, so it is a concurrent collection.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class PresenceService : Contracts.IPresenceService, IPlayerNotifier
    {
        private readonly ConcurrentDictionary<string, OnlinePlayer> _players =
            new ConcurrentDictionary<string, OnlinePlayer>(StringComparer.OrdinalIgnoreCase);

        private readonly LogicLogging.AppLogger _logger;
        private readonly LogicFriends.IPlayerIdFinder _playerIds;
        private readonly LogicSecurity.SessionRegistry _sessions;

        /// <summary>
        /// Initializes a new instance of the PresenceService class.
        /// </summary>
        /// <param name="playerIds">Finds the identifier of the account of a name, to write it in the log.</param>
        /// <param name="sessions">Sessions of the players who logged in, to know who connects.</param>
        public PresenceService(LogicFriends.IPlayerIdFinder playerIds, LogicSecurity.SessionRegistry sessions)
        {
            _playerIds = playerIds ?? throw new ArgumentNullException(nameof(playerIds), LogMessages.NullFinderMessage);
            _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions), LogMessages.NullSessionsMessage);
            _logger = new LogicLogging.AppLogger(GetType());
        }

        /// <summary>
        /// Marks the player of the session as online and warns the other players. A token that is unknown or
        /// that ended is ignored, so nobody can appear online in the name of another player.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player who enters the game.</param>
        /// <returns>The names of all the players who are online, including the caller.</returns>
        public string[] Connect(string sessionToken)
        {
            string[] onlinePlayers = Array.Empty<string>();

            if (_sessions.TryGetPlayerName(sessionToken, out string displayName))
            {
                OnlinePlayer player = CreateCurrentPlayer();

                _players[displayName] = player;
                onlinePlayers = _players.Keys.ToArray();

                WatchChannel(displayName, player.SessionId);
                _logger.Info(string.Format(
                    CultureInfo.InvariantCulture,
                    LogMessages.ConnectedLogFormat,
                    PlayerLogName.Describe(_playerIds, displayName)));
                Broadcast(displayName, Contracts.PresenceEventType.PlayerConnected);
            }
            else
            {
                _logger.Warning(LogMessages.RejectedSessionLogMessage);
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
                _logger.Info(string.Format(
                    CultureInfo.InvariantCulture,
                    LogMessages.DisconnectedLogFormat,
                    PlayerLogName.Describe(_playerIds, displayName)));
                Broadcast(displayName, Contracts.PresenceEventType.PlayerDisconnected);
            }
        }

        private bool RemoveEntry(string displayName, OnlinePlayer player)
        {
            var namedPlayer = new KeyValuePair<string, OnlinePlayer>(displayName, player);

            return ((ICollection<KeyValuePair<string, OnlinePlayer>>)_players).Remove(namedPlayer);
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
