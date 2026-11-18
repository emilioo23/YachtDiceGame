using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// Outcome of asking the server for the challenge of a passkey ceremony.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum PasskeyStartResultCode
    {
        /// <summary>The challenge was issued.</summary>
        [EnumMember]
        Created,

        /// <summary>The player of the session does not exist.</summary>
        [EnumMember]
        PlayerNotFound,

        /// <summary>The player already has the maximum number of passkeys.</summary>
        [EnumMember]
        TooManyPasskeys,

        /// <summary>Too many ceremonies are waiting; the player must try again later.</summary>
        [EnumMember]
        Busy
    }
}
