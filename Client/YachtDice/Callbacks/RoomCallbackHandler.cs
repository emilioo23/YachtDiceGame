using System;
using YachtDice.GameServiceReference;

namespace YachtDice.Callbacks
{
    /// <summary>
    /// Handles room related callbacks from the server.
    /// </summary>
    public class RoomCallbackHandler : IRoomManagerCallback
    {
        /// <summary>
        /// Event triggered when a player's state in the room changes (joined or left).
        /// </summary>
        public event Action<PlayerProfile, RoomEventType> PlayerChanged;

        /// <summary>
        /// Invokes the player changed event.
        /// </summary>
        /// <param name="player">The profile of the player.</param>
        /// <param name="eventType">The type of event that occurred.</param>
        public void OnPlayerChanged(PlayerProfile player, RoomEventType eventType)
        {
            PlayerChanged?.Invoke(player, eventType);
        }
    }
}