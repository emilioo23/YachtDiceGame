using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Outcome of a request for a new verification code, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum ResendResultCode
    {
        /// <summary>A new code was generated and sent.</summary>
        [EnumMember]
        CodeSent,

        /// <summary>The minimum wait since the last code has not passed yet.</summary>
        [EnumMember]
        ResendTooSoon,

        /// <summary>The verification is locked because of too many failed attempts.</summary>
        [EnumMember]
        VerificationLocked,

        /// <summary>A new code was generated but the email could not be sent.</summary>
        [EnumMember]
        CodeSendFailed,

        /// <summary>There is no player registered with the given email.</summary>
        [EnumMember]
        PlayerNotFound
    }
}
