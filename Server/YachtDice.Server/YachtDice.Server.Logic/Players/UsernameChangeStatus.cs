namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Result of trying to change the username of a player.
    /// </summary>
    public enum UsernameChangeStatus
    {
        /// <summary>
        /// The username was changed, or it was already the one requested.
        /// </summary>
        Success,

        /// <summary>
        /// There is no player with the current name.
        /// </summary>
        PlayerNotFound,

        /// <summary>
        /// The new username does not follow the rules of the usernames.
        /// </summary>
        InvalidUsername,

        /// <summary>
        /// The new username contains an offensive word.
        /// </summary>
        OffensiveUsername,

        /// <summary>
        /// Another player already uses the new username.
        /// </summary>
        UsernameAlreadyTaken
    }
}
