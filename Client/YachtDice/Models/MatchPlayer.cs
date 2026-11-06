namespace YachtDice.Models
{
    /// <summary>
    /// Player shown at the top of the scorecard of a match.
    /// </summary>
    public class MatchPlayer
    {
        /// <summary>
        /// Gets or sets the name of the player.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the initial of the name, used for the avatar.
        /// </summary>
        public string Initial { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the player who uses the window.
        /// </summary>
        public bool IsLocalPlayer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player already left the room.
        /// </summary>
        public bool HasLeft { get; set; }
    }
}
