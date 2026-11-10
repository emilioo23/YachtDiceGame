using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Players
{
    /// <summary>
    /// Outcome of asking to use another avatar, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum AvatarChangeResultCode
    {
        /// <summary>The avatar was changed.</summary>
        [EnumMember]
        Success,

        /// <summary>The player does not exist.</summary>
        [EnumMember]
        PlayerNotFound,

        /// <summary>The avatar does not exist.</summary>
        [EnumMember]
        AvatarNotFound,

        /// <summary>The player has not reached the level that the avatar needs.</summary>
        [EnumMember]
        AvatarLocked
    }
}
