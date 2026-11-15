namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Result of trying to delete the account of a player.
    /// </summary>
    public enum AccountDeletionStatus
    {
        /// <summary>
        /// The account and all its data were deleted.
        /// </summary>
        Success,

        /// <summary>
        /// There is no player with that name.
        /// </summary>
        PlayerNotFound,

        /// <summary>
        /// The password that the player typed to confirm the deletion is not the password of the account.
        /// </summary>
        WrongPassword
    }
}
