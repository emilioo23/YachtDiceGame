namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Result of validating a two-step verification code.
    /// </summary>
    public enum TwoFactorValidationStatus
    {
        /// <summary>The code is correct and still valid.</summary>
        Valid,

        /// <summary>The code does not match the issued one.</summary>
        Incorrect,

        /// <summary>The code has expired or none was issued.</summary>
        Expired,

        /// <summary>The email is locked because of too many failed attempts.</summary>
        Locked
    }
}