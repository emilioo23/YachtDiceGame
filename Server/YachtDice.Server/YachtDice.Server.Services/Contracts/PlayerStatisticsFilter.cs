using System.Runtime.Serialization;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Data sent by the client to ask for the statistics of a player.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class PlayerStatisticsFilter
    {
        /// <summary>
        /// Display name or username of the player.
        /// </summary>
        [DataMember]
        public string PlayerName { get; set; }
    }
}