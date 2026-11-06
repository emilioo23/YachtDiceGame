using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Notifications that the server sends back to a client subscribed to <see cref="IRoomService"/>.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IRoomCallback
    {
        /// <summary>
        /// Called when something changes in the room. It always carries the whole state of the room, so the
        /// client only has to show it.
        /// </summary>
        /// <param name="room">State of the room after the change.</param>
        /// <param name="eventType">What happened in the room.</param>
        [OperationContract(IsOneWay = true)]
        void RoomChanged(RoomState room, RoomEventType eventType);
    }
}
