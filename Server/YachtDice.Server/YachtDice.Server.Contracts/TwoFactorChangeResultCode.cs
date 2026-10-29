using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Outcome of the request to turn off the two-step verification, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum TwoFactorChangeResultCode
    {
        /// <summary>The two-step verification was turned off.</summary>
        [EnumMember]
        Success,

        /// <summary>The password is incorrect.</summary>
        [EnumMember]
        InvalidPassword,

        /// <summary>The player does not exist.</summary>
        [EnumMember]
        PlayerNotFound
    }
}
