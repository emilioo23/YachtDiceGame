using System.ServiceModel;
using System.Threading.Tasks;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Operations that the clients can use to join or leave a room.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts, CallbackContract = typeof(IRoomCallback))]
    public interface IRoomManager
    {
        /// <summary>
        /// Adds a player to a room.
        /// </summary>
        /// <param name="player">Profile of the player joining.</param>
        /// <param name="roomId">Identifier of the room.</param>
        /// <returns>A task that completes when the player has been added.</returns>
        [OperationContract]
        Task JoinRoomAsync(PlayerProfile player, string roomId);

        /// <summary>
        /// Removes a player from a room.
        /// </summary>
        /// <param name="username">Username of the player leaving.</param>
        /// <param name="roomId">Identifier of the room.</param>
        /// <returns>A task that completes when the player has been removed.</returns>
        [OperationContract]
        Task LeaveRoomAsync(string username, string roomId);
    }
}
