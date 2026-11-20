namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Outcome of an action in a game and the state of the game after it.
    /// </summary>
    public sealed class GameActionResult
    {
        private GameActionResult(GameActionStatus status, GameSnapshot snapshot)
        {
            Status = status;
            Snapshot = snapshot;
        }

        /// <summary>
        /// Gets the outcome of the action.
        /// </summary>
        public GameActionStatus Status { get; }

        /// <summary>
        /// Gets the state of the game after the action. It is empty if the action failed.
        /// </summary>
        public GameSnapshot Snapshot { get; }

        /// <summary>
        /// Creates the result of an action that could not be done.
        /// </summary>
        /// <param name="status">Reason why the action failed.</param>
        /// <returns>A result with an empty snapshot.</returns>
        public static GameActionResult CreateFailed(GameActionStatus status)
        {
            return new GameActionResult(status, new GameSnapshot());
        }

        /// <summary>
        /// Creates the result of an action that was done.
        /// </summary>
        /// <param name="snapshot">State of the game after the action.</param>
        /// <returns>A successful result.</returns>
        public static GameActionResult CreateSucceeded(GameSnapshot snapshot)
        {
            return new GameActionResult(GameActionStatus.Success, snapshot);
        }
    }
}
