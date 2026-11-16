using System.Collections.Generic;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// A game in progress as it is saved so it can go on after a restart of the server.
    /// </summary>
    public class GameCheckpointRecord
    {
        /// <summary>
        /// Gets or sets the code of the room, the same code that the players used to enter.
        /// </summary>
        public string RoomCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the room.
        /// </summary>
        public string RoomName { get; set; }

        /// <summary>
        /// Gets or sets the most players that the room accepts.
        /// </summary>
        public int MaxPlayers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the room is private.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Gets or sets the name of the speed of the game.
        /// </summary>
        public string Speed { get; set; }

        /// <summary>
        /// Gets or sets the name of the player who has the turn, or an empty text if nobody has it.
        /// </summary>
        public string CurrentPlayerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets how many times the player who has the turn already rolled.
        /// </summary>
        public int RollsMade { get; set; }

        /// <summary>
        /// Gets the players in the order of the turns.
        /// </summary>
        public List<CheckpointPlayerRecord> Players { get; set; } = new List<CheckpointPlayerRecord>();

        /// <summary>
        /// Gets the dice of the current turn, or an empty list if the player did not roll yet.
        /// </summary>
        public List<RolledDieRecord> Dice { get; set; } = new List<RolledDieRecord>();
    }
}
