using System.Collections.Generic;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// A copy of the state of a player in a game, safe to read outside the lock of the game.
    /// </summary>
    public sealed class PlayerSnapshot
    {
        /// <summary>
        /// Initializes a new snapshot.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="name">Name that the screens show for the player.</param>
        /// <param name="isPresent">True if the player is connected to the game.</param>
        public PlayerSnapshot(string playerId, string name, bool isPresent)
        {
            PlayerId = playerId;
            Name = name;
            IsPresent = isPresent;
            Scores = new List<ScoreEntry>();
        }

        /// <summary>
        /// Gets the identifier of the player.
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// Gets the name that the screens show for the player.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the player is connected to the game.
        /// </summary>
        public bool IsPresent { get; }

        /// <summary>
        /// Gets or sets the points of the upper section.
        /// </summary>
        public int UpperSubtotal { get; set; }

        /// <summary>
        /// Gets or sets the bonus of the upper section.
        /// </summary>
        public int UpperBonus { get; set; }

        /// <summary>
        /// Gets or sets the bonus of the extra Yachts.
        /// </summary>
        public int YachtBonus { get; set; }

        /// <summary>
        /// Gets or sets the total of the scorecard.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player won the game. It is only set when the game is
        /// over, and several players can win when they tie.
        /// </summary>
        public bool IsWinner { get; set; }

        /// <summary>
        /// Gets or sets the experience that the player earned in the game. It is only set when the game is
        /// over, and it stays at zero for the players who left before the end.
        /// </summary>
        public int ExperienceGained { get; set; }

        /// <summary>
        /// Gets the boxes of the scorecard, in the order of the scorecard.
        /// </summary>
        public IList<ScoreEntry> Scores { get; }
    }
}
