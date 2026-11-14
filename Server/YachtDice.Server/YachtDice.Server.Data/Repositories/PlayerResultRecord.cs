namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Result of one registered player in a game that just finished.
    /// </summary>
    public class PlayerResultRecord
    {
        /// <summary>
        /// Gets or sets the identifier of the player in the database.
        /// </summary>
        public int PlayerId { get; set; }

        /// <summary>
        /// Gets or sets the total points of the player when the game finished.
        /// </summary>
        public int FinalScore { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player won the game.
        /// </summary>
        public bool IsWinner { get; set; }

        /// <summary>
        /// Gets or sets how the player ended the game.
        /// </summary>
        public PlayerResultStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the accumulated experience of the player, counting the experience of this game.
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        /// Gets or sets the level of the player that matches the accumulated experience.
        /// </summary>
        public int Level { get; set; }
    }
}
