namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// State of a player of a game in progress, as it is stored in the RoomPlayer table.
    /// </summary>
    public enum RoomPlayerRecordStatus
    {
        /// <summary>
        /// The player still takes part in the game.
        /// </summary>
        Playing,

        /// <summary>
        /// The player did not come back in time and lost the game.
        /// </summary>
        Forfeited
    }
}
