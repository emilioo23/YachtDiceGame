using System;
using System.Collections.Generic;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Result of the request to create new backup codes.
    /// </summary>
    public class BackupCodeCreation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackupCodeCreation"/> class.
        /// </summary>
        /// <param name="status">Outcome of the request.</param>
        /// <param name="codes">The new codes; empty if they were not created.</param>
        public BackupCodeCreation(BackupCodeCreationStatus status, List<string> codes)
        {
            Status = status;
            Codes = codes ?? throw new ArgumentNullException(nameof(codes), LogMessages.NullCodesMessage);
        }

        /// <summary>
        /// Outcome of the request.
        /// </summary>
        public BackupCodeCreationStatus Status { get; }

        /// <summary>
        /// The new codes, which the player sees only once; empty if they were not created.
        /// </summary>
        public List<string> Codes { get; }
    }
}
