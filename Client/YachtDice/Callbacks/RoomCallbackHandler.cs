using System;
using YachtDice.RoomServiceReference;

namespace YachtDice.Callbacks
{
    /// <summary>
    /// Handles room related callbacks from the server.
    /// </summary>
    public class RoomCallbackHandler : IRoomServiceCallback
    {
        /// <summary>
        /// Event triggered when a player's state in the room changes (joined or left).
        /// </summary>
        public event Action<PlayerProfile, RoomEventType> PlayerChanged;

        /// <summary>
        /// Raises the player changed event when the server reports a change in the room.
        /// </summary>
        /// <param name="player">The profile of the player.</param>
        /// <param name="eventType">The type of event that occurred.</param>
        void IRoomServiceCallback.PlayerChanged(PlayerProfile player, RoomEventType eventType)
        {
            PlayerChanged?.Invoke(player, eventType);
        }
    }
}
