using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// Outcome of saving a new passkey.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum PasskeyRegistrationResultCode
    {
        /// <summary>The passkey was saved.</summary>
        [EnumMember]
        Registered,

        /// <summary>The answer of the authenticator is not valid.</summary>
        [EnumMember]
        InvalidResponse,

        /// <summary>The challenge was already used or expired.</summary>
        [EnumMember]
        ChallengeExpired,

        /// <summary>The player already has the maximum number of passkeys.</summary>
        [EnumMember]
        TooManyPasskeys,

        /// <summary>The credential is already registered.</summary>
        [EnumMember]
        AlreadyRegistered,

        /// <summary>The player of the session does not exist.</summary>
        [EnumMember]
        PlayerNotFound
    }
}
