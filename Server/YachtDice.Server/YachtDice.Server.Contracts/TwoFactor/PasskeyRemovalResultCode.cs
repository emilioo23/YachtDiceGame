using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// Outcome of removing a passkey.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum PasskeyRemovalResultCode
    {
        /// <summary>The passkey was removed.</summary>
        [EnumMember]
        Removed,

        /// <summary>The password is not the right one.</summary>
        [EnumMember]
        InvalidPassword,

        /// <summary>The player has no passkey with that identifier.</summary>
        [EnumMember]
        PasskeyNotFound,

        /// <summary>The player of the session does not exist.</summary>
        [EnumMember]
        PlayerNotFound
    }
}
