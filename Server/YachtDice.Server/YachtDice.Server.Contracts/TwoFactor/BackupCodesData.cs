using System.Collections.Generic;
using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// Answer of the server to the request to create new backup codes.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class BackupCodesData
    {
        /// <summary>
        /// Outcome of the request.
        /// </summary>
        [DataMember]
        public BackupCodesResultCode Code { get; set; }

        /// <summary>
        /// The new codes, which the server does not keep in clear text; empty if they were not created.
        /// </summary>
        [DataMember]
        public List<string> Codes { get; set; }
    }
}
