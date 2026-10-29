using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Notifications that the server sends back to a client subscribed to <see cref="IPresenceService"/>.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IPresenceCallback
    {
        /// <summary>
        /// Called when another player enters or leaves the game.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="eventType">What happened to the player.</param>
        [OperationContract(IsOneWay = true)]
        void PresenceChanged(string displayName, PresenceEventType eventType);
    }
}
