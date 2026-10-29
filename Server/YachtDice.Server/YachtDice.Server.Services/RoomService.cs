using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;

    /// <summary>
    /// Manages which players are in each room and tells the others when someone joins or leaves.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class RoomService : Contracts.IRoomService
    {
        private readonly CallbackRegistry<Contracts.IRoomCallback> _callbacks =
            new CallbackRegistry<Contracts.IRoomCallback>();

        /// <summary>
        /// Adds a player to a room and notifies every client in it.
        /// </summary>
        /// <param name="player">Profile of the player who joins.</param>
        /// <param name="roomId">Identifier of the room.</param>
        public void JoinRoom(Contracts.PlayerProfile player, string roomId)
        {
            _callbacks.Register(roomId, OperationContext.Current.GetCallbackChannel<Contracts.IRoomCallback>());

            _callbacks.Notify(
                roomId,
                callback => callback.PlayerChanged(player, Contracts.RoomEventType.PlayerJoined));
        }

        /// <summary>
        /// Removes a player from a room and notifies the remaining clients.
        /// </summary>
        /// <param name="player">Profile of the player who leaves. Only the username is required.</param>
        /// <param name="roomId">Identifier of the room.</param>
        public void LeaveRoom(Contracts.PlayerProfile player, string roomId)
        {
            _callbacks.Remove(roomId, OperationContext.Current.GetCallbackChannel<Contracts.IRoomCallback>());

            _callbacks.Notify(
                roomId,
                callback => callback.PlayerChanged(player, Contracts.RoomEventType.PlayerLeft));
        }
    }
}
