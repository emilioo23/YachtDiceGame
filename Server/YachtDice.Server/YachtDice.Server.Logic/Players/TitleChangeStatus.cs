namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Result of trying to choose the title that a player shows in the profile.
    /// </summary>
    public enum TitleChangeStatus
    {
        /// <summary>
        /// The title was saved.
        /// </summary>
        Success,

        /// <summary>
        /// There is no player with that name.
        /// </summary>
        PlayerNotFound,

        /// <summary>
        /// The title does not exist.
        /// </summary>
        TitleNotFound,

        /// <summary>
        /// The player has not unlocked the title yet.
        /// </summary>
        TitleLocked
    }
}
