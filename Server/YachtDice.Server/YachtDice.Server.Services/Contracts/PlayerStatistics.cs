using System.Runtime.Serialization;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Summary of the games played by a player, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class PlayerStatistics
    {
        /// <summary>
        /// Indicates whether the player exists; if not, the other values are empty.
        /// </summary>
        [DataMember]
        public bool IsPlayerFound { get; set; }

        /// <summary>
        /// Number of games played.
        /// </summary>
        [DataMember]
        public int GamesPlayed { get; set; }

        /// <summary>
        /// Number of games won.
        /// </summary>
        [DataMember]
        public int Wins { get; set; }

        /// <summary>
        /// Highest score obtained in a game.
        /// </summary>
        [DataMember]
        public int BestScore { get; set; }

        /// <summary>
        /// Percentage of games won, rounded to a whole number.
        /// </summary>
        [DataMember]
        public int WinRatePercentage { get; set; }

        /// <summary>
        /// Current level of the player.
        /// </summary>
        [DataMember]
        public int Level { get; set; }
    }
}