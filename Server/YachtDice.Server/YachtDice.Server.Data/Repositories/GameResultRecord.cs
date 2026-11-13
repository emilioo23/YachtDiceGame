using System.Collections.Generic;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Result of a game that just finished, with the registered players who took part in it.
    /// </summary>
    public class GameResultRecord
    {
        /// <summary>
        /// Initializes a result without players.
        /// </summary>
        public GameResultRecord()
        {
            Players = new List<PlayerResultRecord>();
        }

        /// <summary>
        /// Gets or sets the identifier in the database of the player who owns the room of the game.
        /// </summary>
        public int HostPlayerId { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of players that the room allowed.
        /// </summary>
        public int MaxPlayers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the room of the game was private.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Gets the result of each registered player of the game.
        /// </summary>
        public IList<PlayerResultRecord> Players { get; }
    }
}
