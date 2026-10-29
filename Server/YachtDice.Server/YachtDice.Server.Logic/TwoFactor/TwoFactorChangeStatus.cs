namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Outcome of the request to turn off the two-step verification.
    /// </summary>
    public enum TwoFactorChangeStatus
    {
        /// <summary>The two-step verification was turned off.</summary>
        Success,

        /// <summary>The password is incorrect.</summary>
        InvalidPassword,

        /// <summary>The player does not exist.</summary>
        PlayerNotFound
    }
}
