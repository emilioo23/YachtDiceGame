using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Players
{
    /// <summary>
    /// Result of trying to delete the account of a player.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum AccountDeletionResultCode
    {
        /// <summary>
        /// The account and all its data were deleted.
        /// </summary>
        [EnumMember]
        Success,

        /// <summary>
        /// There is no player with that name.
        /// </summary>
        [EnumMember]
        PlayerNotFound,

        /// <summary>
        /// The password that the player typed to confirm the deletion is not the password of the account.
        /// </summary>
        [EnumMember]
        WrongPassword
    }
}
