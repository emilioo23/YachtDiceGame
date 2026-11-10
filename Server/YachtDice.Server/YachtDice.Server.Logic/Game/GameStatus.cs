namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Phase of a game.
    /// </summary>
    public enum GameStatus
    {
        /// <summary>The players are still playing.</summary>
        InProgress,

        /// <summary>No present player has open boxes, so the game is over.</summary>
        Finished
    }
}
