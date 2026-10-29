using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Operations that let the clients tell the server they are online and know who else is.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts, CallbackContract = typeof(IPresenceCallback))]
    public interface IPresenceService
    {
        /// <summary>
        /// Marks the player as online and warns the other players.
        /// </summary>
        /// <param name="displayName">Name of the player who enters the game.</param>
        /// <returns>The names of all the players who are online, including the caller.</returns>
        [OperationContract]
        string[] Connect(string displayName);

        /// <summary>
        /// Marks the player as offline and warns the other players.
        /// </summary>
        /// <param name="displayName">Name of the player who leaves the game.</param>
        [OperationContract(IsOneWay = true)]
        void Disconnect(string displayName);
    }
}
