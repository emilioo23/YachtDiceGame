namespace YachtDice.Models
{
    /// <summary>
    /// One game of the history, ready to be shown in the list.
    /// </summary>
    public class HistoryRow
    {
        /// <summary>
        /// Gets or sets a value indicating whether the player won the game.
        /// </summary>
        public bool IsWin { get; set; }

        /// <summary>
        /// Gets or sets the text that tells whether the player won or lost.
        /// </summary>
        public string ResultText { get; set; }

        /// <summary>
        /// Gets or sets the text with the names of the other players.
        /// </summary>
        public string OpponentsText { get; set; }

        /// <summary>
        /// Gets or sets the text with the score of the player and the best score of the others.
        /// </summary>
        public string ScoreText { get; set; }

        /// <summary>
        /// Gets or sets the text with the date and time of the game.
        /// </summary>
        public string DateText { get; set; }
    }
}
