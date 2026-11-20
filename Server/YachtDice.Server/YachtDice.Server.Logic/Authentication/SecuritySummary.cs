using System;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// What the player sees about the security of the account: the last change of password and the last lock.
    /// </summary>
    public class SecuritySummary
    {
        /// <summary>
        /// Gets or sets a value indicating whether the player ever changed the password.
        /// </summary>
        public bool HasPasswordChange { get; set; }

        /// <summary>
        /// Gets or sets the moment of the last change of password, in universal time.
        /// </summary>
        public DateTime LastPasswordChange { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the account was locked in the last days or is still locked.
        /// </summary>
        public bool HasRecentLock { get; set; }

        /// <summary>
        /// Gets or sets the moment when the last lock ends or ended, in universal time.
        /// </summary>
        public DateTime LockedUntil { get; set; }

        /// <summary>
        /// Gets or sets the kind of the last lock: Temporary, Daily or Reported.
        /// </summary>
        public string LockReason { get; set; }
    }
}
