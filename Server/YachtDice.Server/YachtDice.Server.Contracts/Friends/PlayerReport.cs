using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Friends
{
    /// <summary>
    /// What a player says about another player when reporting them.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class PlayerReport
    {
        /// <summary>
        /// Name of the player who is reported.
        /// </summary>
        [DataMember]
        public string ReportedName { get; set; }

        /// <summary>
        /// Reason chosen by the player who reports.
        /// </summary>
        [DataMember]
        public ReportReason Reason { get; set; }

        /// <summary>
        /// Optional comment that explains what happened.
        /// </summary>
        [DataMember]
        public string Comment { get; set; }
    }
}
