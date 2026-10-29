using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Callbacks;
using YachtDice.PresenceServiceReference;
using YachtDice.Utils;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Keeps the player connected to the presence service while the game is open and
    /// knows which players are online. There is only one connection for the whole application.
    /// </summary>
    public class PresenceGateway
    {
        private const string UnavailableLogMessage = "The presence service could not be reached.";
        private const string UnavailableMessage = "The server is not available.";

        private readonly object _syncRoot = new object();
        private readonly HashSet<string> _onlinePlayers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly AppLogger _logger = new AppLogger(typeof(PresenceGateway));
        private PresenceServiceClient _client;
        private string _displayName;

        private PresenceGateway()
        {
        }

        /// <summary>
        /// Event triggered when the list of online players changes. It can be raised from any thread.
        /// </summary>
        public event Action PlayersChanged;

        /// <summary>
        /// Gets the connection shared by all the windows.
        /// </summary>
        public static PresenceGateway Current { get; } = new PresenceGateway();

        /// <summary>
        /// Gets the names of the players who are online right now.
        /// </summary>
        /// <returns>A copy of the list of online players.</returns>
        public List<string> GetOnlinePlayers()
        {
            lock (_syncRoot)
            {
                return _onlinePlayers.ToList();
            }
        }

        /// <summary>
        /// Connects the player to the presence service, unless it is already connected.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>A task that finishes when the player is online.</returns>
        /// <exception cref="GatewayException">The server could not be reached.</exception>
        public async Task ConnectAsync(string displayName)
        {
            if (!IsConnectedAs(displayName))
            {
                await Task.Run(() => Disconnect());
                await OpenConnectionAsync(displayName);
            }
        }

        /// <summary>
        /// Tells the server that the player left and closes the connection.
        /// </summary>
        public void Disconnect()
        {
            PresenceServiceClient client;
            string displayName;

            lock (_syncRoot)
            {
                client = _client;
                displayName = _displayName;
                _client = null;
                _displayName = null;
                _onlinePlayers.Clear();
            }

            if (client != null)
            {
                CloseClient(client, displayName);
            }

            PlayersChanged?.Invoke();
        }

        private static void CloseClient(PresenceServiceClient client, string displayName)
        {
            try
            {
                client.Disconnect(displayName);
                client.Close();
            }
            catch (CommunicationException)
            {
                client.Abort();
            }
            catch (TimeoutException)
            {
                client.Abort();
            }
        }

        private bool IsConnectedAs(string displayName)
        {
            lock (_syncRoot)
            {
                return _client != null && string.Equals(_displayName, displayName, StringComparison.OrdinalIgnoreCase);
            }
        }

        private async Task OpenConnectionAsync(string displayName)
        {
            var handler = new PresenceCallbackHandler();
            handler.PresenceChanged += HandlePresenceChanged;

            var client = new PresenceServiceClient(new InstanceContext(handler));
            ServerAddress.Apply(client.Endpoint);

            try
            {
                string[] names = await Task.Run(() => client.Connect(displayName));

                client.InnerChannel.Faulted += (sender, args) => HandleConnectionLost(client);
                SaveConnection(client, displayName, names);
            }
            catch (CommunicationException ex)
            {
                client.Abort();
                _logger.Error(ex, UnavailableLogMessage);
                throw new GatewayException(GatewayErrorCode.ServerUnavailable, UnavailableMessage, ex);
            }
            catch (TimeoutException ex)
            {
                client.Abort();
                _logger.Error(ex, UnavailableLogMessage);
                throw new GatewayException(GatewayErrorCode.ServerUnavailable, UnavailableMessage, ex);
            }
        }

        private void SaveConnection(PresenceServiceClient client, string displayName, IEnumerable<string> names)
        {
            lock (_syncRoot)
            {
                _client = client;
                _displayName = displayName;
                _onlinePlayers.Clear();
                _onlinePlayers.UnionWith(names);
            }

            PlayersChanged?.Invoke();
        }

        private void HandlePresenceChanged(string displayName, PresenceEventType eventType)
        {
            lock (_syncRoot)
            {
                if (eventType == PresenceEventType.PlayerConnected)
                {
                    _onlinePlayers.Add(displayName);
                }
                else
                {
                    _onlinePlayers.Remove(displayName);
                }
            }

            PlayersChanged?.Invoke();
        }

        private void HandleConnectionLost(PresenceServiceClient client)
        {
            lock (_syncRoot)
            {
                if (ReferenceEquals(_client, client))
                {
                    _client = null;
                    _displayName = null;
                    _onlinePlayers.Clear();
                }
            }

            client.Abort();
            PlayersChanged?.Invoke();
        }
    }
}
