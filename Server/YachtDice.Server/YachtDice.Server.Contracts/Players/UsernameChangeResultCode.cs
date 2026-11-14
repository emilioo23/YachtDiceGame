using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Players
{
    /// <summary>
    /// Result of trying to change the username of a player.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum UsernameChangeResultCode
    {
        /// <summary>
        /// The username was changed.
        /// </summary>
        [EnumMember]
        Success,

        /// <summary>
        /// There is no player with the current name.
        /// </summary>
        [EnumMember]
        PlayerNotFound,

        /// <summary>
        /// The new username does not follow the rules of the usernames.
        /// </summary>
        [EnumMember]
        InvalidUsername,

        /// <summary>
        /// The new username contains an offensive word.
        /// </summary>
        [EnumMember]
        OffensiveUsername,

        /// <summary>
        /// Another player already uses the new username.
        /// </summary>
        [EnumMember]
        UsernameAlreadyTaken
    }
}
