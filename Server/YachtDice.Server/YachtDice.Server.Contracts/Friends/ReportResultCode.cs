using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Friends
{
    /// <summary>
    /// Outcome of a report about another player, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum ReportResultCode
    {
        /// <summary>The report was saved.</summary>
        [EnumMember]
        Sent,

        /// <summary>One of the two players does not have an account.</summary>
        [EnumMember]
        PlayerNotFound,

        /// <summary>The player tried to report themselves.</summary>
        [EnumMember]
        CannotReportSelf,

        /// <summary>The two players neither finished a game together recently nor play one now.</summary>
        [EnumMember]
        NoRecentGame,

        /// <summary>The player already reported the same player recently.</summary>
        [EnumMember]
        AlreadyReported,

        /// <summary>The comment is too long.</summary>
        [EnumMember]
        InvalidComment
    }
}
