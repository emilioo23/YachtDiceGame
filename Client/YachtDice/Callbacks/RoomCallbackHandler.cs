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
        /// Event triggered when a player joins the room.
        /// </summary>
        public event Action<PlayerProfile> PlayerJoined;

        /// <summary>
        /// Event triggered when a player leaves the room.
        /// </summary>
        public event Action<string> PlayerLeft;

        /// <summary>
        /// Invokes the player joined event.
        /// </summary>
        /// <param name="player">The profile of the player that joined.</param>
        public void OnPlayerJoined(PlayerProfile player)
        {
            PlayerJoined?.Invoke(player);
        }

        /// <summary>
        /// Invokes the player left event.
        /// </summary>
        /// <param name="username">The username of the player that left.</param>
        public void OnPlayerLeft(string username)
        {
            PlayerLeft?.Invoke(username);
        }
    }
}