namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Outcome of the request to turn on the two-step verification.
    /// </summary>
    public enum TwoFactorActivationStatus
    {
        /// <summary>The verification code was sent to the player's email.</summary>
        CodeSent,

        /// <summary>The player does not exist.</summary>
        PlayerNotFound,

        /// <summary>The two-step verification is already turned on.</summary>
        AlreadyEnabled,

        /// <summary>The verification is locked because of too many failed attempts.</summary>
        VerificationLocked,

        /// <summary>The verification code could not be sent.</summary>
        CodeSendFailed
    }
}
