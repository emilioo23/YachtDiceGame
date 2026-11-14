using System;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// What one player scored in a game that already finished.
    /// </summary>
    public class PlayedGameRecord
    {
        /// <summary>
        /// Gets or sets the identifier of the game.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Gets or sets the moment when the game ended, if it was saved.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the player.
        /// </summary>
        public int PlayerId { get; set; }

        /// <summary>
        /// Gets or sets the name that the other players see.
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Gets or sets the score of the player at the end of the game.
        /// </summary>
        public int FinalScore { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player won the game.
        /// </summary>
        public bool IsWinner { get; set; }
    }
}
