namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// What the window must show about the turn of the game.
    /// </summary>
    public enum TurnPhase
    {
        /// <summary>
        /// The game is over.
        /// </summary>
        Finished,

        /// <summary>
        /// Nobody can play because all the players are away.
        /// </summary>
        Paused,

        /// <summary>
        /// It is the turn of the player who uses the window.
        /// </summary>
        LocalTurn,

        /// <summary>
        /// It is the turn of another player.
        /// </summary>
        OpponentTurn
    }
}
