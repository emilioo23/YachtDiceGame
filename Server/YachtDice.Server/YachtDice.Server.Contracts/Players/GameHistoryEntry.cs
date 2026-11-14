using System;
using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Players
{
    /// <summary>
    /// A game that a player already played, as it is shown in the history.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class GameHistoryEntry
    {
        /// <summary>
        /// Moment when the game ended.
        /// </summary>
        [DataMember]
        public DateTime PlayedAt { get; set; }

        /// <summary>
        /// True if the player won the game.
        /// </summary>
        [DataMember]
        public bool IsWin { get; set; }

        /// <summary>
        /// Score of the player.
        /// </summary>
        [DataMember]
        public int OwnScore { get; set; }

        /// <summary>
        /// Highest score among the other players.
        /// </summary>
        [DataMember]
        public int BestOpponentScore { get; set; }

        /// <summary>
        /// Names of the other players, separated by commas.
        /// </summary>
        [DataMember]
        public string Opponents { get; set; }
    }
}
