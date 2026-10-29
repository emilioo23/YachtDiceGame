using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Defines the service contract for registering new players.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IPlayerRegistrationService
    {
        /// <summary>
        /// Registers a new player.
        /// </summary>
        /// <param name="newPlayer">Data of the player who wants to register.</param>
        /// <returns>Outcome of the registration.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        RegisterResultCode RegisterPlayer(Player newPlayer);
    }
}