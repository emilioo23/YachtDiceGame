using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.RegistrationServiceReference;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the registration operations of the server.
    /// </summary>
    public class RegistrationGateway : GatewayBase<PlayerRegistrationServiceClient>
    {
        /// <summary>
        /// Registers a new player.
        /// </summary>
        /// <param name="newPlayer">Data of the new player.</param>
        /// <returns>Outcome of the registration.</returns>
        public Task<RegisterResultCode> RegisterPlayerAsync(Player newPlayer)
        {
            return ExecuteAsync(client => client.RegisterPlayer(newPlayer));
        }

        /// <inheritdoc />
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            return GatewayErrorCode.UnexpectedError;
        }
    }
}