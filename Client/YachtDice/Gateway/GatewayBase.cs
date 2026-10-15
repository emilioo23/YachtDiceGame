using System;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Utils;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Common behavior of the gateways: creates the proxy, runs the operation
    /// and converts any communication failure into a <see cref="GatewayException"/>.
    /// </summary>
    /// <typeparam name="TClient">Generated proxy used to reach the server.</typeparam>
    public abstract class GatewayBase<TClient>
        where TClient : ICommunicationObject, new()
    {
        private const string FaultLogMessage = "The server reported an error.";
        private const string UnavailableLogMessage = "The server could not be reached.";
        private const string FaultMessage = "The server could not complete the request.";
        private const string UnavailableMessage = "The server is not available.";

        private readonly AppLogger _logger;

        /// <summary>
        /// Initializes the gateway.
        /// </summary>
        protected GatewayBase()
        {
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Translates the fault sent by the server into the error code understood by the client.
        /// </summary>
        /// <param name="fault">Fault received from the server.</param>
        /// <returns>Error code that represents the fault.</returns>
        protected abstract GatewayErrorCode GetErrorCode(FaultException fault);

        /// <summary>
        /// Runs an operation against the server and releases the proxy afterwards.
        /// </summary>
        /// <typeparam name="TResponse">Type returned by the operation.</typeparam>
        /// <param name="operation">Call to execute using the proxy.</param>
        /// <returns>Response sent by the server.</returns>
        /// <exception cref="GatewayException">The request could not be completed.</exception>
        protected async Task<TResponse> ExecuteAsync<TResponse>(Func<TClient, Task<TResponse>> operation)
        {
            var client = new TClient();
            TResponse response;

            try
            {
                response = await operation(client);
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

            return response;
        }
    }
}