namespace YachtDice.Server.Logic.Avatars
{
    /// <summary>
    /// Tells which avatar a player is using, so the screens of the other players can show it.
    /// </summary>
    public interface IAvatarLookup
    {
        /// <summary>
        /// Gets the avatar of the player who owns the name.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The identifier of the avatar, or the default avatar if the player is not registered.</returns>
        int GetAvatarId(string displayName);
    }
}
