using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Points the endpoints of the generated WCF clients to the server configured in the application settings.
    /// </summary>
    internal static class ServerAddress
    {
        private const string ServerIpKey = "ServerIp";
        private const string DefaultServerIp = "localhost";

        /// <summary>
        /// Replaces the host of the endpoint with the server address of the application settings.
        /// </summary>
        /// <param name="endpoint">Endpoint of a generated WCF client.</param>
        public static void Apply(ServiceEndpoint endpoint)
        {
            string serverIp = ConfigurationManager.AppSettings[ServerIpKey] ?? DefaultServerIp;
            var builder = new UriBuilder(endpoint.Address.Uri) { Host = serverIp };

            endpoint.Address = new EndpointAddress(
                builder.Uri,
                endpoint.Address.Identity,
                endpoint.Address.Headers);
        }
    }
}
