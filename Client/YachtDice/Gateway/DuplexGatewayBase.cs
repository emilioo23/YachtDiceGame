using System;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Utils;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Common behavior of the gateways that keep one connection open with the server because the server
    /// also sends notifications to the client (duplex services). It runs each call without blocking the
    /// interface and converts the failures into <see cref="GatewayException"/>. Its methods are called from
    /// the thread of the interface, but the channel tells when it breaks from another thread, so the client
    /// is only changed inside the lock.
    /// </summary>
    /// <typeparam name="TClient">Type of the generated WCF client that the gateway uses.</typeparam>
    public abstract class DuplexGatewayBase<TClient> : IConnectionLossSource
        where TClient : class, ICommunicationObject
    {
        private const string FaultLogMessage = "The server reported an error.";
        private const string UnavailableLogMessage = "The server could not be reached.";
        private const string FaultMessage = "The server could not complete the request.";
        private const string UnavailableMessage = "The server is not available.";
        private const string NotConnectedMessage = "There is no connection with the server.";
        private const string ConnectionLostLogMessage = "The connection with the server was lost.";

        private readonly object _syncRoot = new object();
        private readonly AppLogger _logger;
        private TClient _client;

        /// <summary>
        /// Initializes the logger with the type of the gateway that inherits from this class.
        /// </summary>
        protected DuplexGatewayBase()
        {
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Event triggered when the connection with the server breaks without the application closing it. It
        /// can be raised from any thread.
        /// </summary>
        public event Action ConnectionLost;

        /// <summary>
        /// Gets whether the connection with the server is open.
        /// </summary>
        protected bool IsConnected
        {
            get
            {
                return CurrentClient() != null;
            }
        }

        /// <summary>
        /// Closes the connection with the server, if it is open.
        /// </summary>
        /// <returns>A task that finishes when the connection is closed.</returns>
        public async Task CloseAsync()
        {
            TClient client = TakeClient();

            if (client != null)
            {
                await Task.Run(() => CloseGracefully(client));
            }
        }

        /// <summary>
        /// Creates the WCF client of the gateway, already subscribed to the notifications of the server.
        /// </summary>
        /// <returns>A client that is ready to be used.</returns>
        protected abstract TClient CreateClient();

        /// <summary>
        /// Translates a fault sent by the server into the error code that the interface understands.
        /// </summary>
        /// <param name="fault">Fault received from the server.</param>
        /// <returns>The error code that matches the fault.</returns>
        protected abstract GatewayErrorCode GetErrorCode(FaultException fault);

        /// <summary>
        /// Opens a new connection and makes the first call on it. If the call fails, the connection is closed.
        /// </summary>
        /// <typeparam name="TResult">Type of the answer of the call.</typeparam>
        /// <param name="operation">Call to make with the WCF client.</param>
        /// <returns>The answer of the server.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        protected async Task<TResult> ConnectAndExecuteAsync<TResult>(Func<TClient, TResult> operation)
        {
            TResult result;

            SaveClient(CreateClient());

            try
            {
                result = await ExecuteAsync(operation);
            }
            catch (GatewayException)
            {
                await CloseAsync();
                throw;
            }

            return result;
        }

        /// <summary>
        /// Opens a new connection and makes the first call on it. If the call fails, the connection is closed.
        /// </summary>
        /// <param name="operation">Call to make with the WCF client.</param>
        /// <returns>A task that finishes when the server answers.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        protected async Task ConnectAndExecuteAsync(Action<TClient> operation)
        {
            await ConnectAndExecuteAsync(client => RunOperation(client, operation));
        }

        /// <summary>
        /// Makes a call to the server through the connection that is already open.
        /// </summary>
        /// <typeparam name="TResult">Type of the answer of the call.</typeparam>
        /// <param name="operation">Call to make with the WCF client.</param>
        /// <returns>The answer of the server.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        protected async Task<TResult> ExecuteAsync<TResult>(Func<TClient, TResult> operation)
        {
            TClient client = RequireClient();
            TResult result;

            try
            {
                result = await Task.Run(() => operation(client));
            }
            catch (FaultException ex)
            {
                _logger.Error(ex, FaultLogMessage);
                throw new GatewayException(GetErrorCode(ex), FaultMessage, ex);
            }
            catch (CommunicationException ex)
            {
                DiscardClient(client);
                _logger.Error(ex, UnavailableLogMessage);
                throw new GatewayException(GatewayErrorCode.ServerUnavailable, UnavailableMessage, ex);
            }
            catch (TimeoutException ex)
            {
                DiscardClient(client);
                _logger.Error(ex, UnavailableLogMessage);
                throw new GatewayException(GatewayErrorCode.ServerUnavailable, UnavailableMessage, ex);
            }

            return result;
        }

        /// <summary>
        /// Makes a call that gives no answer through the connection that is already open.
        /// </summary>
        /// <param name="operation">Call to make with the WCF client.</param>
        /// <returns>A task that finishes when the call is done.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        protected async Task ExecuteAsync(Action<TClient> operation)
        {
            await ExecuteAsync(client => RunOperation(client, operation));
        }

        private static bool RunOperation(TClient client, Action<TClient> operation)
        {
            operation(client);

            return true;
        }

        private static void CloseGracefully(TClient client)
        {
            try
            {
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

        private TClient RequireClient()
        {
            TClient client = CurrentClient();

            if (client == null)
            {
                throw new GatewayException(GatewayErrorCode.ServerUnavailable, NotConnectedMessage, null);
            }

            return client;
        }

        private TClient CurrentClient()
        {
            lock (_syncRoot)
            {
                return _client;
            }
        }

        private TClient TakeClient()
        {
            TClient client;

            lock (_syncRoot)
            {
                client = _client;
                _client = null;
            }

            return client;
        }

        private bool TryDetach(TClient client)
        {
            bool isCurrent;

            lock (_syncRoot)
            {
                isCurrent = ReferenceEquals(_client, client);

                if (isCurrent)
                {
                    _client = null;
                }
            }

            return isCurrent;
        }

        private void SaveClient(TClient client)
        {
            client.Faulted += (sender, e) => HandleConnectionBroken(client);
            client.Closed += (sender, e) => HandleConnectionBroken(client);

            lock (_syncRoot)
            {
                _client = client;
            }
        }

        private void HandleConnectionBroken(TClient client)
        {
            if (TryDetach(client))
            {
                client.Abort();
                _logger.Warning(ConnectionLostLogMessage);
                ConnectionLost?.Invoke();
            }
        }

        private void DiscardClient(TClient client)
        {
            TryDetach(client);
            client.Abort();
        }
    }
}
