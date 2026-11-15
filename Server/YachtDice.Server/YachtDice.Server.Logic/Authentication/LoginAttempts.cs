using System;
using System.Collections.Generic;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// The wrong passwords of one account. The lock that they cause is saved in the database. It is not
    /// thread-safe:
    /// <see cref="LoginAttemptTracker"/> only uses it while it holds its lock.
    /// </summary>
    internal sealed class LoginAttempts
    {
        /// <summary>
        /// Initializes the attempts of an account without failures.
        /// </summary>
        public LoginAttempts()
        {
            FailureTimes = new List<DateTime>();
        }

        /// <summary>
        /// Gets or sets the number of wrong passwords since the last lock or the last correct one.
        /// </summary>
        public int ConsecutiveFailures { get; set; }

        /// <summary>
        /// Gets the moments, in UTC, of the wrong passwords of the last twenty-four hours.
        /// </summary>
        public List<DateTime> FailureTimes { get; }
    }
}
