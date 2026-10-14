using System;

namespace YachtDice.Services
{
    /// <summary>
    /// In-memory state of a verification code issued to an email address.
    /// </summary>
    internal class TwoFactorCodeEntry
    {
        /// <summary>
        /// Numeric code issued to the player.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Moment (UTC) when the code was generated; used for the resend cooldown.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Moment (UTC) from which the code is no longer valid.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Failed attempts accumulated before the lockout is applied.
        /// </summary>
        public int FailedAttempts { get; set; }

        /// <summary>
        /// Moment (UTC) until which the email stays locked.
        /// </summary>
        public DateTime LockedUntil { get; set; }
    }
}