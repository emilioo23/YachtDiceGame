using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Outcome of the first step of the login, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum LoginResultCode
    {
        /// <summary>The credentials are correct and the verification code was sent.</summary>
        [EnumMember]
        VerificationCodeSent,

        /// <summary>The email or the password was not provided.</summary>
        [EnumMember]
        MissingRequiredField,

        /// <summary>The email or the password is incorrect.</summary>
        [EnumMember]
        InvalidCredentials,

        /// <summary>The two-step verification is locked because of too many failed attempts.</summary>
        [EnumMember]
        VerificationLocked,

        /// <summary>The credentials are correct but the verification code could not be sent.</summary>
        [EnumMember]
        CodeSendFailed,

        /// <summary>The credentials are correct and the player has no two-step verification.</summary>
        [EnumMember]
        LoggedIn
    }
}
