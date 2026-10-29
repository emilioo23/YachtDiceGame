using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using YachtDice.Utils;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Common behavior of the gateways: it runs each call to the server without blocking the interface
    /// and converts the failures into <see cref="GatewayException"/>.
    /// </summary>
    /// <typeparam name="TClient">Type of the generated WCF client that the gateway uses.</typeparam>
    public abstract class GatewayBase<TClient>
        where TClient : ICommunicationObject, new()
    {
        private const string FaultLogMessage = "The server reported an error.";
        private const string UnavailableLogMessage = "The server could not be reached.";
        private const string FaultMessage = "The server could not complete the request.";
        private const string UnavailableMessage = "The server is not available.";
        private const string ServerIpKey = "ServerIp";
        private const string DefaultServerIp = "localhost";
        private const string EndpointPropertyName = "Endpoint";

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
        /// <typeparam name="TResponse">Type of the answer of the call.</typeparam>
        /// <param name="operation">Call to make with the WCF client.</param>
        /// <returns>The answer of the server.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        protected async Task<TResponse> ExecuteAsync<TResponse>(Func<TClient, TResponse> operation)
        {
            TClient client = CreateConfiguredClient();
            TResponse response;

            try
            {
                response = await Task.Run(() => operation(client));
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

        private TClient CreateConfiguredClient()
        {
            var client = new TClient();
            var endpointProperty = client.GetType().GetProperty(EndpointPropertyName);

            if (endpointProperty?.GetValue(client, null) is ServiceEndpoint endpoint)
            {
                string serverIp = ConfigurationManager.AppSettings[ServerIpKey] ?? DefaultServerIp;
                var builder = new UriBuilder(endpoint.Address.Uri) { Host = serverIp };

                endpoint.Address = new EndpointAddress(
                    builder.Uri,
                    endpoint.Address.Identity,
                    endpoint.Address.Headers);
            }

            return client;
        }
    }
}
