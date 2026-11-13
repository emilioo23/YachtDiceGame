namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// The player who has the turn and the player who uses the window, to know whose turn it is.
    /// </summary>
    public sealed class TurnPlayers
    {
        /// <summary>
        /// Initializes the two players.
        /// </summary>
        /// <param name="currentPlayerId">Identifier of the player in turn; empty if nobody can play.</param>
        /// <param name="localPlayerId">Identifier of the player who uses the window.</param>
        public TurnPlayers(string currentPlayerId, string localPlayerId)
        {
            CurrentPlayerId = currentPlayerId;
            LocalPlayerId = localPlayerId;
        }

        /// <summary>
        /// Gets the identifier of the player in turn; empty if nobody can play.
        /// </summary>
        public string CurrentPlayerId { get; }

        /// <summary>
        /// Gets the identifier of the player who uses the window.
        /// </summary>
        public string LocalPlayerId { get; }
    }
}
