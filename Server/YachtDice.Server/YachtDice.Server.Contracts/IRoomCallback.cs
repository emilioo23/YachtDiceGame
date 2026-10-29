using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Notifications that the server sends back to a client subscribed to <see cref="IRoomManager"/>.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IRoomCallback
    {
        /// <summary>
        /// Called when a player's status in the room changes (e.g., joins or leaves).
        /// </summary>
        /// <param name="player">Profile of the player involved. If leaving, it may only contain the username.</param>
        /// <param name="eventType">The specific action that occurred.</param>
        [OperationContract(IsOneWay = true)]
        void OnPlayerChanged(PlayerProfile player, RoomEventType eventType);
    }
}