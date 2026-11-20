using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// Outcome of the request to create new backup codes, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum BackupCodesResultCode
    {
        /// <summary>The new codes were created and the old ones stopped working.</summary>
        [EnumMember]
        Created,

        /// <summary>The password is incorrect.</summary>
        [EnumMember]
        InvalidPassword,

        /// <summary>The player does not have the two-step verification turned on.</summary>
        [EnumMember]
        TwoFactorDisabled,

        /// <summary>The player does not exist.</summary>
        [EnumMember]
        PlayerNotFound
    }
}
