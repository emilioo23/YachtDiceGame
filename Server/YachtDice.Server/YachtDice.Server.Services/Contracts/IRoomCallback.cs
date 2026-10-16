using System.ServiceModel;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Notifications that the server sends back to a client subscribed to <see cref="IRoomManager"/>.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IRoomCallback
    {
        /// <summary>
        /// Called when another player joins the room.
        /// </summary>
        /// <param name="player">Profile of the player who joined.</param>
        [OperationContract(IsOneWay = true)]
        void OnPlayerJoined(PlayerProfile player);

        /// <summary>
        /// Called when another player leaves the room.
        /// </summary>
        /// <param name="username">Username of the player who left.</param>
        [OperationContract(IsOneWay = true)]
        void OnPlayerLeft(string username);
    }
}
