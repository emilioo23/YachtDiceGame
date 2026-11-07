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
    /// the thread of the interface and they continue there after each await, so the client needs no lock.
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
        public async Task CloseAsync()
        {
            TClient client = _client;

            _client = null;

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

            _client = CreateClient();

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
            if (_client == null)
            {
                throw new GatewayException(GatewayErrorCode.ServerUnavailable, NotConnectedMessage, null);
            }

            return _client;
        }

        private void DiscardClient(TClient client)
        {
            if (ReferenceEquals(_client, client))
            {
                _client = null;
            }

            client.Abort();
        }
    }
}
