using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.RegistrationServiceReference;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the registration operations of the server.
    /// </summary>
    public class RegistrationGateway : GatewayBase<PlayerRegistrationServiceClient, IPlayerRegistrationService>
    {
        /// <summary>
        /// Registers a new player.
        /// </summary>
        /// <param name="newPlayer">Data of the new player.</param>
        /// <returns>Outcome of the registration.</returns>
        public async Task<RegisterResultCode> RegisterPlayerAsync(Player newPlayer)
        {
            RegisterResultCode result = await ExecuteAsync(client => client.RegisterPlayer(newPlayer));

            return result;
        }

        /// <inheritdoc />
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<ServiceFault>;

            return GatewayErrorCodeParser.Parse(typedFault?.Detail.Code);
        }
    }
}