using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Friends
{
    /// <summary>
    /// Reasons that a player can choose to report another player, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum ReportReason
    {
        /// <summary>The player used insults or offensive words.</summary>
        [EnumMember]
        OffensiveLanguage,

        /// <summary>The player seemed to cheat.</summary>
        [EnumMember]
        Cheating,

        /// <summary>The player behaved badly on purpose.</summary>
        [EnumMember]
        UnsportingBehavior,

        /// <summary>Any other reason, explained in the comment.</summary>
        [EnumMember]
        Other
    }
}
