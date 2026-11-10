using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Authentication
{
    /// <summary>
    /// Outcome of the confirmation of a password recovery, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum PasswordResetResultCode
    {
        /// <summary>The code was correct and the password was changed.</summary>
        [EnumMember]
        PasswordChanged,

        /// <summary>The email, the code or the new password was not written.</summary>
        [EnumMember]
        MissingRequiredField,

        /// <summary>The code does not match the one that was sent.</summary>
        [EnumMember]
        IncorrectCode,

        /// <summary>The code has expired or none was requested.</summary>
        [EnumMember]
        CodeExpired,

        /// <summary>Too many wrong codes were entered, so the code was discarded.</summary>
        [EnumMember]
        TooManyAttempts
    }
}
