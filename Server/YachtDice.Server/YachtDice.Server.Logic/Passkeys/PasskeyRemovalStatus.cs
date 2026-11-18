namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Outcome of removing a passkey.
    /// </summary>
    public enum PasskeyRemovalStatus
    {
        /// <summary>The passkey was removed.</summary>
        Removed,

        /// <summary>The password that the player typed is not the right one.</summary>
        InvalidPassword,

        /// <summary>The player has no passkey with that identifier.</summary>
        PasskeyNotFound,

        /// <summary>The player of the session does not exist.</summary>
        PlayerNotFound
    }
}
