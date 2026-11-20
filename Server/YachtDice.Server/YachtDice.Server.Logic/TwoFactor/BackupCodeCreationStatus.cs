namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Outcome of the request to create new backup codes.
    /// </summary>
    public enum BackupCodeCreationStatus
    {
        /// <summary>The new codes were created and the old ones stopped working.</summary>
        Created,

        /// <summary>The password is incorrect.</summary>
        InvalidPassword,

        /// <summary>
        /// The player does not have the two-step verification turned on, so the codes are not needed.
        /// </summary>
        TwoFactorDisabled,

        /// <summary>The player does not exist.</summary>
        PlayerNotFound
    }
}
