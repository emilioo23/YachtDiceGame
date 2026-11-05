using System;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Utils;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Common behavior of the gateways that keep one connection open with the server because the server
    /// also sends notifications to the client (duplex services). It runs each call without blocking the
    /// interface and converts the failures into <see cref="GatewayException"/>.
    /// </summary>
    /// <typeparam name="TClient">Type of the generated WCF client that the gateway uses.</typeparam>
    public abstract class DuplexGatewayBase<TClient>
        where TClient : class, ICommunicationObject
    {
        private const string FaultLogMessage = "The server reported an error.";
        private const string UnavailableLogMessage = "The server could not be reached.";
        private const string FaultMessage = "The server could not complete the request.";
        private const string UnavailableMessage = "The server is not available.";
        private const string NotConnectedMessage = "There is no connection with the server.";

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
        /// Closes the connection with the server, if it is open.
        /// </summary>
        /// <returns>A task that finishes when the connection is closed.</returns>
        public Task CloseAsync()
        {
            return Task.Run(() => CloseClient());
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
        /// <typeparam name="TResponse">Type of the answer of the call.</typeparam>
        /// <param name="operation">Call to make with the WCF client.</param>
        /// <returns>The answer of the server.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        protected async Task<TResponse> ConnectAndExecuteAsync<TResponse>(Func<TClient, TResponse> operation)
        {
            TResponse response;

            SaveClient(CreateClient());

            try
            {
                response = await ExecuteAsync(operation);
            }
            catch (GatewayException)
            {
                await CloseAsync();
                throw;
            }

            return response;
        }

        /// <summary>
        /// Opens a new connection and makes the first call on it. If the call fails, the connection is closed.
        /// </summary>
        /// <param name="operation">Call to make with the WCF client.</param>
        /// <returns>A task that finishes when the server answers.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        protected Task ConnectAndExecuteAsync(Action<TClient> operation)
        {
            return ConnectAndExecuteAsync(client => RunOperation(client, operation));
        }

        /// <summary>
        /// Makes a call to the server through the connection that is already open.
        /// </summary>
        /// <typeparam name="TResponse">Type of the answer of the call.</typeparam>
        /// <param name="operation">Call to make with the WCF client.</param>
        /// <returns>The answer of the server.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        protected async Task<TResponse> ExecuteAsync<TResponse>(Func<TClient, TResponse> operation)
        {
            TClient client = GetClient();
            TResponse response;

            try
            {
                response = await Task.Run(() => operation(client));
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

            return response;
        }

        /// <summary>
        /// Makes a call that gives no answer through the connection that is already open.
        /// </summary>
        /// <param name="operation">Call to make with the WCF client.</param>
        /// <returns>A task that finishes when the call is done.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        protected Task ExecuteAsync(Action<TClient> operation)
        {
            return ExecuteAsync(client => RunOperation(client, operation));
        }

        /// <summary>
        /// Closes the connection with the server. It blocks while the server answers, so call it from a
        /// background thread.
        /// </summary>
        protected void CloseClient()
        {
            TClient client;

            lock (_syncRoot)
            {
                client = _client;
                _client = null;
            }

            if (client != null)
            {
                CloseGracefully(client);
            }
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

        private void SaveClient(TClient client)
        {
            lock (_syncRoot)
            {
                _client = client;
            }
        }

        private TClient GetClient()
        {
            TClient client;

            lock (_syncRoot)
            {
                client = _client;
            }

            if (client == null)
            {
                throw new GatewayException(GatewayErrorCode.ServerUnavailable, NotConnectedMessage, null);
            }

            return client;
        }

        private void DiscardClient(TClient client)
        {
            lock (_syncRoot)
            {
                if (ReferenceEquals(_client, client))
                {
                    _client = null;
                }
            }

            client.Abort();
        }
    }
}
