namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Outcome of a request to send the verification code again.
    /// </summary>
    public enum ResendCodeStatus
    {
        /// <summary>A new code was generated and sent.</summary>
        CodeSent,

        /// <summary>The minimum wait since the last code has not passed yet.</summary>
        ResendTooSoon,

        /// <summary>The verification is locked because of too many failed attempts.</summary>
        VerificationLocked,

        /// <summary>A new code was generated but the email could not be sent.</summary>
        CodeSendFailed,

        /// <summary>There is no player registered with the given email.</summary>
        PlayerNotFound
    }
}