using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.RegistrationServiceReference;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the registration operations of the server.
    /// </summary>
    public class RegistrationGateway : GatewayBase<PlayerRegistrationManagerClient>
    {
        /// <summary>
        /// Registers a new player.
        /// </summary>
        /// <param name="request">Data of the new player.</param>
        /// <returns>Outcome of the registration.</returns>
        public Task<RegisterResultCode> RegisterAsync(PlayerRegistrationInfo request)
        {
            return ExecuteAsync(client => client.RegisterAsync(request));
        }

        /// <inheritdoc />
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            return GatewayErrorCode.UnexpectedError;
        }
    }
}