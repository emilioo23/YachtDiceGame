using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Authentication
{
    /// <summary>
    /// Outcome of a request to receive a password recovery code, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum PasswordResetRequestResultCode
    {
        /// <summary>The request was accepted, whether or not the email has an account.</summary>
        [EnumMember]
        CodeSent,

        /// <summary>The email was not written.</summary>
        [EnumMember]
        MissingRequiredField,

        /// <summary>The code was generated but the email could not be sent.</summary>
        [EnumMember]
        CodeSendFailed
    }
}
