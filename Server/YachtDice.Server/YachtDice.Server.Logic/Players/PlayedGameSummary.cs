using System;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Summary of a game that a player already played, as it is shown in the history.
    /// </summary>
    public class PlayedGameSummary
    {
        /// <summary>
        /// Gets or sets the moment when the game ended.
        /// </summary>
        public DateTime PlayedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player won the game.
        /// </summary>
        public bool IsWin { get; set; }

        /// <summary>
        /// Gets or sets the score of the player.
        /// </summary>
        public int OwnScore { get; set; }

        /// <summary>
        /// Gets or sets the highest score among the other players.
        /// </summary>
        public int BestOpponentScore { get; set; }

        /// <summary>
        /// Gets or sets the names of the other players, separated by commas.
        /// </summary>
        public string Opponents { get; set; }
    }
}
