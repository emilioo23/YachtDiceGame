using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Outcome of the request to turn on the two-step verification, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum TwoFactorActivationResultCode
    {
        /// <summary>The verification code was sent to the player's email.</summary>
        [EnumMember]
        CodeSent,

        /// <summary>The player does not exist.</summary>
        [EnumMember]
        PlayerNotFound,

        /// <summary>The two-step verification is already turned on.</summary>
        [EnumMember]
        AlreadyEnabled,

        /// <summary>The verification is locked because of too many failed attempts.</summary>
        [EnumMember]
        VerificationLocked,

        /// <summary>The verification code could not be sent.</summary>
        [EnumMember]
        CodeSendFailed
    }
}
