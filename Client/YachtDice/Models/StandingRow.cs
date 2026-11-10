namespace YachtDice.Models
{
    /// <summary>
    /// Row of the final scoreboard: the place, the player and the points.
    /// </summary>
    public class StandingRow
    {
        /// <summary>
        /// Gets or sets the medal or the number that marks the place of the player.
        /// </summary>
        public string PlaceText { get; set; }

        /// <summary>
        /// Gets or sets the name of the player.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the points of the player.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player won the game.
        /// </summary>
        public bool IsWinner { get; set; }
    }
}
