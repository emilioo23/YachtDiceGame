using System.Collections.Generic;
using YachtDice.Gateway;

namespace YachtDice.Models
{
    /// <summary>
    /// Player who is using the application. It goes from one window to the next one through their constructors.
    /// </summary>
    public class PlayerSession
    {
        /// <summary>
        /// Initializes the session of the player.
        /// </summary>
        /// <param name="displayName">Name that identifies the player in the game.</param>
        /// <param name="firstName">First name of the player, used to greet them. It is empty for a guest.</param>
        /// <param name="isGuest">Whether the player entered as a guest, without an account.</param>
        public PlayerSession(string displayName, string firstName, bool isGuest)
        {
            DisplayName = displayName;
            FirstName = firstName;
            IsGuest = isGuest;
            Presence = new PresenceGateway();
            LastOfferedMatchCode = string.Empty;
        }

        /// <summary>
        /// Gets the name that identifies the player in the game.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the first name of the player. It is empty for a guest.
        /// </summary>
        public string FirstName { get; }

        /// <summary>
        /// Gets a value indicating whether the player entered as a guest, without an account.
        /// </summary>
        public bool IsGuest { get; }

        /// <summary>
        /// Gets the connection of the player to the presence service, shared by all the windows.
        /// </summary>
        public PresenceGateway Presence { get; }

        /// <summary>
        /// Gets or sets the code of the last match in progress that the menu offered to the player, so the
        /// offer is not repeated every time the menu opens.
        /// </summary>
        public string LastOfferedMatchCode { get; set; }

        /// <summary>
        /// Gets or sets the achievements that the player is known to have unlocked, or null before they are first read.
        /// </summary>
        public List<int> KnownAchievementIds { get; set; }
    }
}
