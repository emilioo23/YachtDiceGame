namespace YachtDice.Server.Logic.Avatars
{
    /// <summary>
    /// Result of asking to use another avatar.
    /// </summary>
    public enum AvatarChangeStatus
    {
        /// <summary>
        /// The avatar was changed.
        /// </summary>
        Success,

        /// <summary>
        /// The player does not exist.
        /// </summary>
        PlayerNotFound,

        /// <summary>
        /// The avatar does not exist in the catalog.
        /// </summary>
        AvatarNotFound,

        /// <summary>
        /// The player has not reached the level that the avatar needs.
        /// </summary>
        AvatarLocked
    }
}
