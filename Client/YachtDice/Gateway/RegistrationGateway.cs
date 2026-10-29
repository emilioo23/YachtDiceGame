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
        /// <param name="player">Data of the new player.</param>
        /// <returns>Outcome of the registration.</returns>
        public Task<RegisterResultCode> RegisterAsync(Player player)
        {
            return ExecuteAsync(client => client.RegisterAsync(player));
        }

        /// <inheritdoc />
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            return GatewayErrorCode.UnexpectedError;
        }
    }
}