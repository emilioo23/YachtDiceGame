using System;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Utils;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Common behavior of the gateways: it runs each call to the server without blocking the interface
    /// and converts the failures into <see cref="GatewayException"/>.
    /// </summary>
    /// <typeparam name="TClient">Type of the generated WCF client that the gateway uses.</typeparam>
    /// <typeparam name="TChannel">Service contract of the generated WCF client.</typeparam>
    public abstract class GatewayBase<TClient, TChannel>
        where TClient : ClientBase<TChannel>, new()
        where TChannel : class
    {
        private const string FaultLogMessage = "The server reported an error.";
        private const string UnavailableLogMessage = "The server could not be reached.";
        private const string FaultMessage = "The server could not complete the request.";
        private const string UnavailableMessage = "The server is not available.";

        private readonly AppLogger _logger;

        /// <summary>
        /// Initializes the logger with the type of the gateway that inherits from this class.
        /// </summary>
        protected GatewayBase()
        {
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Translates a fault sent by the server into the error code that the interface understands.
        /// </summary>
        /// <param name="fault">Fault received from the server.</param>
        /// <returns>The error code that matches the fault.</returns>
        protected abstract GatewayErrorCode GetErrorCode(FaultException fault);

        /// <summary>
        /// Runs a call to the server in the background and closes the client when it finishes.
        /// </summary>
        /// <typeparam name="TResult">Type of the answer of the call.</typeparam>
        /// <param name="operation">Call to make with the WCF client.</param>
        /// <returns>The answer of the server.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        protected async Task<TResult> ExecuteAsync<TResult>(Func<TClient, TResult> operation)
        {
            TClient client = CreateConfiguredClient();
            TResult result;

            try
            {
                result = await Task.Run(() => operation(client));
                client.Close();
            }
            catch (FaultException ex)
            {
                _logger.Error(ex, FaultLogMessage);
                throw new GatewayException(GetErrorCode(ex), FaultMessage, ex);
            }
            catch (CommunicationException ex)
            {
                _logger.Error(ex, UnavailableLogMessage);
                throw new GatewayException(GatewayErrorCode.ServerUnavailable, UnavailableMessage, ex);
            }
            catch (TimeoutException ex)
            {
                _logger.Error(ex, UnavailableLogMessage);
                throw new GatewayException(GatewayErrorCode.ServerUnavailable, UnavailableMessage, ex);
            }
            finally
            {
                if (client.State != CommunicationState.Closed)
                {
                    client.Abort();
                }
            }

            return result;
        }

        private static TClient CreateConfiguredClient()
        {
            var client = new TClient();

            ServerAddress.Apply(client.Endpoint);

            return client;
        }
    }
}
