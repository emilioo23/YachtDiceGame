using System.Collections.Generic;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// A copy of the state of a game, safe to read outside the lock of the game manager.
    /// </summary>
    public sealed class GameSnapshot
    {
        /// <summary>
        /// Initializes an empty snapshot.
        /// </summary>
        public GameSnapshot()
        {
            CurrentPlayerId = string.Empty;
            Dice = new List<int>();
            HeldDice = new List<bool>();
            Players = new List<PlayerSnapshot>();
            Options = new List<ScoreEntry>();
        }

        /// <summary>
        /// Gets or sets the phase of the game.
        /// </summary>
        public GameStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the number of the snapshot. It grows every time a snapshot of the same game is made,
        /// so a client can tell which of two snapshots is the newest.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the player whose turn it is. It is empty when nobody can play.
        /// </summary>
        public string CurrentPlayerId { get; set; }

        /// <summary>
        /// Gets or sets the number of the turn. It grows each time the turn passes to another player.
        /// </summary>
        public int TurnNumber { get; set; }

        /// <summary>
        /// Gets or sets the round of the player whose turn it is, from one to twelve.
        /// </summary>
        public int Round { get; set; }

        /// <summary>
        /// Gets or sets how many rolls the current player has left in the turn.
        /// </summary>
        public int RollsLeft { get; set; }

        /// <summary>
        /// Gets or sets the seconds that a player has to finish the turn.
        /// </summary>
        public int TurnSeconds { get; set; }

        /// <summary>
        /// Gets the values of the dice. It is empty until the current player rolls.
        /// </summary>
        public IList<int> Dice { get; }

        /// <summary>
        /// Gets which dice the current player held in the last roll.
        /// </summary>
        public IList<bool> HeldDice { get; }

        /// <summary>
        /// Gets the players, in the order of the turns.
        /// </summary>
        public IList<PlayerSnapshot> Players { get; }

        /// <summary>
        /// Gets the boxes that the current player can choose with the current dice.
        /// </summary>
        public IList<ScoreEntry> Options { get; }
    }
}
