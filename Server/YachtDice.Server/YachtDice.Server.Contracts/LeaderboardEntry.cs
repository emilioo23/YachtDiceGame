using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// One row of the ranking of players, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class LeaderboardEntry
    {
        /// <summary>
        /// Place of the player in the ranking, starting at 1.
        /// </summary>
        [DataMember]
        public int Position { get; set; }

        /// <summary>
        /// Display name of the player.
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// Sum of the final scores of all the games of the player.
        /// </summary>
        [DataMember]
        public int TotalPoints { get; set; }

        /// <summary>
        /// Number of games won by the player.
        /// </summary>
        [DataMember]
        public int Wins { get; set; }
    }
}
