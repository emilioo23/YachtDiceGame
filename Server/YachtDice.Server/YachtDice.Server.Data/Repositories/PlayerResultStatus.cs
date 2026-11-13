namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// How a player ended a game.
    /// </summary>
    public enum PlayerResultStatus
    {
        /// <summary>
        /// The player was in the game when it finished.
        /// </summary>
        Completed,

        /// <summary>
        /// The player was not in the game when it finished.
        /// </summary>
        Abandoned
    }
}
