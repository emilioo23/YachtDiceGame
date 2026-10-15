using System;
using System.Configuration;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Utils;

namespace YachtDice.Gateway
{
    public abstract class GatewayBase<TClient>
        where TClient : ICommunicationObject, new()
    {
        private const string FaultLogMessage = "The server reported an error.";
        private const string UnavailableLogMessage = "The server could not be reached.";
        private const string FaultMessage = "The server could not complete the request.";
        private const string UnavailableMessage = "The server is not available.";
        private const string ServerIpKey = "ServerIp";
        private const string DefaultServerIp = "localhost";

        private readonly AppLogger _logger;

        protected GatewayBase()
        {
            _logger = new AppLogger(GetType());
        }

        protected abstract GatewayErrorCode GetErrorCode(FaultException fault);

        protected async Task<TResponse> ExecuteAsync<TResponse>(Func<TClient, Task<TResponse>> operation)
        {
            var client = CreateConfiguredClient();
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

        private TClient CreateConfiguredClient()
        {
            var client = new TClient();
            var endpointProperty = client.GetType().GetProperty("Endpoint");

            if (endpointProperty?.GetValue(client, null) is System.ServiceModel.Description.ServiceEndpoint endpoint)
            {
                string serverIp = ConfigurationManager.AppSettings[ServerIpKey] ?? DefaultServerIp;
                var builder = new UriBuilder(endpoint.Address.Uri) { Host = serverIp };
                endpoint.Address = new EndpointAddress(builder.Uri, endpoint.Address.Identity, endpoint.Address.Headers);
            }

            return client;
        }
    }
}