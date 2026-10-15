using System.Runtime.Serialization;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Outcome of a player registration, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum RegisterResultCode
    {
        /// <summary>The player was registered.</summary>
        [EnumMember]
        Success,

        /// <summary>At least one required field is empty.</summary>
        [EnumMember]
        MissingRequiredField,

        /// <summary>At least one field is longer than allowed.</summary>
        [EnumMember]
        FieldTooLong,

        /// <summary>The email address does not have a valid format.</summary>
        [EnumMember]
        InvalidEmail,

        /// <summary>Another player already uses the email.</summary>
        [EnumMember]
        EmailAlreadyRegistered,

        /// <summary>Another player already uses the username.</summary>
        [EnumMember]
        UsernameAlreadyTaken
    }
}