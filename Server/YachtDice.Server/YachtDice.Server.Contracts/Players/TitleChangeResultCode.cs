using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Players
{
    /// <summary>
    /// Result of trying to choose the title that a player shows in the profile.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum TitleChangeResultCode
    {
        /// <summary>
        /// The title was saved.
        /// </summary>
        [EnumMember]
        Success,

        /// <summary>
        /// There is no player with that name.
        /// </summary>
        [EnumMember]
        PlayerNotFound,

        /// <summary>
        /// The title does not exist.
        /// </summary>
        [EnumMember]
        TitleNotFound,

        /// <summary>
        /// The player has not unlocked the title yet.
        /// </summary>
        [EnumMember]
        TitleLocked
    }
}
