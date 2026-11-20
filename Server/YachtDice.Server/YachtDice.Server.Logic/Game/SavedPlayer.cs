using System.Collections.Generic;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// A player of a game that was saved before the server stopped.
    /// </summary>
    public sealed class SavedPlayer
    {
        /// <summary>
        /// Gets or sets the identifier that the room gives to the player when the game is restored.
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the player.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player already lost for not coming back in time.
        /// </summary>
        public bool HasForfeited { get; set; }

        /// <summary>
        /// Gets or sets how many extra yachts the player made.
        /// </summary>
        public int YachtBonusCount { get; set; }

        /// <summary>
        /// Gets the boxes that the player already filled.
        /// </summary>
        public List<ScoreEntry> Scores { get; } = new List<ScoreEntry>();
    }
}
