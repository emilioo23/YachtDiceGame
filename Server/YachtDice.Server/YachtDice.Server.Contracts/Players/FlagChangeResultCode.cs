using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Players
{
    /// <summary>
    /// Result of trying to choose the country flag that a player shows in the profile.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum FlagChangeResultCode
    {
        /// <summary>
        /// The flag was saved, or removed when the code was empty.
        /// </summary>
        [EnumMember]
        Success,

        /// <summary>
        /// There is no player with that name.
        /// </summary>
        [EnumMember]
        PlayerNotFound,

        /// <summary>
        /// The code is not made of two letters.
        /// </summary>
        [EnumMember]
        InvalidFlag
    }
}
