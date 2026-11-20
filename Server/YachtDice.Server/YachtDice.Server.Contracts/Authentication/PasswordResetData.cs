using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Authentication
{
    /// <summary>
    /// Data the player writes to finish a password recovery.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class PasswordResetData
    {
        /// <summary>
        /// Email of the account to recover.
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Code received by email.
        /// </summary>
        [DataMember]
        public string Code { get; set; }

        /// <summary>
        /// New password in plain text.
        /// </summary>
        [DataMember]
        public string NewPassword { get; set; }
    }
}
