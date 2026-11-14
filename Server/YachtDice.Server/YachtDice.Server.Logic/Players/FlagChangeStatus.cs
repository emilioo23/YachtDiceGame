namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Result of trying to choose the country flag that a player shows in the profile.
    /// </summary>
    public enum FlagChangeStatus
    {
        /// <summary>
        /// The flag was saved, or removed when the code was empty.
        /// </summary>
        Success,

        /// <summary>
        /// There is no player with that name.
        /// </summary>
        PlayerNotFound,

        /// <summary>
        /// The code is not made of two letters.
        /// </summary>
        InvalidFlag
    }
}
