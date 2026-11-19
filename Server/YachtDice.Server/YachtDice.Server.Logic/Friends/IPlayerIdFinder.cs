namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Finds the identifier of the account that owns a name.
    /// </summary>
    public interface IPlayerIdFinder
    {
        /// <summary>
        /// Finds the identifier of the account that owns a name.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The identifier, or <see cref="FriendshipService.NoPlayerId"/> if the name has no account.</returns>
        int FindPlayerId(string displayName);
    }
}
