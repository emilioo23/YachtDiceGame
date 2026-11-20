using System;
using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// What the player sees about the security of the account: the last change of password and the last lock.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class SecuritySummary
    {
        /// <summary>
        /// Whether the player ever changed the password.
        /// </summary>
        [DataMember]
        public bool HasPasswordChange { get; set; }

        /// <summary>
        /// Moment of the last change of password, in universal time.
        /// </summary>
        [DataMember]
        public DateTime LastPasswordChange { get; set; }

        /// <summary>
        /// Whether the account was locked in the last days or is still locked.
        /// </summary>
        [DataMember]
        public bool HasRecentLock { get; set; }

        /// <summary>
        /// Moment when the last lock ends or ended, in universal time.
        /// </summary>
        [DataMember]
        public DateTime LockedUntil { get; set; }

        /// <summary>
        /// Kind of the last lock: Temporary, Daily or Reported.
        /// </summary>
        [DataMember]
        public string LockReason { get; set; }
    }
}
