namespace YachtDice.Server.Logic.Security
{
    /// <summary>
    /// Rules of the codes that the server sends by email to recover a password or to verify a login, so both kinds
    /// of codes behave the same way.
    /// </summary>
    public static class VerificationCodeRules
    {
        /// <summary>
        /// Number of digits of a code.
        /// </summary>
        public const int CodeLength = 6;

        /// <summary>
        /// Minutes that a code can be used after it is sent.
        /// </summary>
        public const int ExpirationMinutes = 10;

        /// <summary>
        /// Wrong codes that can be entered before the code stops working.
        /// </summary>
        public const int MaxFailedAttempts = 3;

        /// <summary>
        /// Seconds that the player must wait before asking for a new code.
        /// </summary>
        public const int ResendCooldownSeconds = 30;
    }
}
