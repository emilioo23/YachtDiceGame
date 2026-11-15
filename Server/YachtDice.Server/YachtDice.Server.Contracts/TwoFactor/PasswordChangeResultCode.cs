using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// Result of a password change asked from the settings.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum PasswordChangeResultCode
    {
        /// <summary>The password was changed.</summary>
        [EnumMember]
        Success,

        /// <summary>The account does not exist anymore.</summary>
        [EnumMember]
        PlayerNotFound,

        /// <summary>The current password is not the right one.</summary>
        [EnumMember]
        WrongCurrentPassword,

        /// <summary>The new password is too short, too long or does not have a letter and a digit.</summary>
        [EnumMember]
        InvalidNewPassword,

        /// <summary>The new password is the current one or one of the last ones.</summary>
        [EnumMember]
        PasswordUsedBefore,

        /// <summary>The account is locked because of too many wrong passwords.</summary>
        [EnumMember]
        TooManyAttempts
    }
}
