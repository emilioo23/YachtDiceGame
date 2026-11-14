using System.Collections.Generic;
using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Game
{
    /// <summary>
    /// Current state of a game, as seen by the clients. It always carries the whole game, so a client that
    /// joins late or comes back only has to show it.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class GameState
    {
        /// <summary>
        /// Phase of the game.
        /// </summary>
        [DataMember]
        public GameStatus Status { get; set; }

        /// <summary>
        /// Number that grows with every change of the game. A client ignores a state with a lower number
        /// than the last one it showed, because notifications can arrive out of order.
        /// </summary>
        [DataMember]
        public int Version { get; set; }

        /// <summary>
        /// Identifier of the player whose turn it is. It is empty when nobody can play.
        /// </summary>
        [DataMember]
        public string CurrentPlayerId { get; set; }

        /// <summary>
        /// Number of the turn. It grows each time the turn passes to another player.
        /// </summary>
        [DataMember]
        public int TurnNumber { get; set; }

        /// <summary>
        /// Round of the player whose turn it is, from one to twelve.
        /// </summary>
        [DataMember]
        public int Round { get; set; }

        /// <summary>
        /// Rolls that the current player has left in the turn.
        /// </summary>
        [DataMember]
        public int RollsLeft { get; set; }

        /// <summary>
        /// Seconds that a player has to play a turn.
        /// </summary>
        [DataMember]
        public int TurnSeconds { get; set; }

        /// <summary>
        /// Seconds left for the current turn when the state was created.
        /// </summary>
        [DataMember]
        public int TurnSecondsLeft { get; set; }

        /// <summary>
        /// Values of the five dice. It is empty until the current player rolls.
        /// </summary>
        [DataMember]
        public List<int> Dice { get; set; }

        /// <summary>
        /// Which dice the current player held in the last roll.
        /// </summary>
        [DataMember]
        public List<bool> HeldDice { get; set; }

        /// <summary>
        /// The players, in the order of the turns.
        /// </summary>
        [DataMember]
        public List<PlayerScorecard> Players { get; set; }

        /// <summary>
        /// Boxes that the current player can choose with the current dice, and the points of each one.
        /// </summary>
        [DataMember]
        public List<ScoreChoice> Options { get; set; }

        /// <summary>
        /// Whether the result of the finished game could not be saved. When it is true, nobody earned
        /// experience in the game.
        /// </summary>
        [DataMember]
        public bool HasResultSaveFailed { get; set; }
    }
}
