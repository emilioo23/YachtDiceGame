using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Operations that the clients can use to register a new player.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IPlayerRegistrationManager
    {
        /// <summary>
        /// Registers a new player in the system.
        /// </summary>
        /// <param name="player">The player entity to register.</param>
        /// <returns>A code indicating the result of the registration attempt.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        RegisterResultCode Register(Player player);
    }
}