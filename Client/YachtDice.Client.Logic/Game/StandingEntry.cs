namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// A player and the points that the player has at the end of a game.
    /// </summary>
    public sealed class StandingEntry
    {
        /// <summary>
        /// Initializes a new entry.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="name">Name that the screens show for the player.</param>
        /// <param name="total">Total points of the player.</param>
        public StandingEntry(string playerId, string name, int total)
        {
            PlayerId = playerId;
            Name = name;
            Total = total;
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
        /// Gets the total points of the player.
        /// </summary>
        public int Total { get; }
    }
}
