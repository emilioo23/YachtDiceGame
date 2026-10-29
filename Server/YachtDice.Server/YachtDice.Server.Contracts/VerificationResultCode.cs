using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Result of validating a verification code, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum VerificationResultCode
    {
        /// <summary>The code is correct and still valid.</summary>
        [EnumMember]
        Valid,

        /// <summary>The code does not match the issued one.</summary>
        [EnumMember]
        Incorrect,

        /// <summary>The code has expired or none was issued.</summary>
        [EnumMember]
        Expired,

        /// <summary>The email is locked because of too many failed attempts.</summary>
        [EnumMember]
        Locked
    }
}
