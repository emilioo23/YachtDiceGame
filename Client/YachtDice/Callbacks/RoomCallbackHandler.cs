using System;
using System.ServiceModel;
using YachtDice.RoomServiceReference;

namespace YachtDice.Callbacks
{
    /// <summary>
    /// Handles the room callbacks from the server: players who enter or leave, new options and the start of
    /// the game.
    /// </summary>
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class RoomCallbackHandler : IRoomServiceCallback
    {
        /// <summary>
        /// Event triggered when something changes in the room. It is raised from a network thread.
        /// </summary>
        public event Action<RoomInfo, RoomEventType> RoomChanged;

        /// <summary>
        /// Raises the room changed event when the server reports a change.
        /// </summary>
        /// <param name="room">State of the room after the change.</param>
        /// <param name="eventType">What happened in the room.</param>
        void IRoomServiceCallback.RoomChanged(RoomInfo room, RoomEventType eventType)
        {
            RoomChanged?.Invoke(room, eventType);
        }
    }
}
