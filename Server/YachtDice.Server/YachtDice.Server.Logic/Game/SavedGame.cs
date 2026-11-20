using System.Collections.Generic;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// A game that was saved before the server stopped, with what is needed to go on from the same turn.
    /// </summary>
    public sealed class SavedGame
    {
        /// <summary>
        /// Gets or sets the code of the room, the same code that the players used before the restart.
        /// </summary>
        public string RoomCode { get; set; }

        /// <summary>
        /// Gets or sets the settings of the room.
        /// </summary>
        public RoomSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the name of the player who had the turn, or an empty text if nobody had it.
        /// </summary>
        public string CurrentPlayerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets how many times the player who had the turn already rolled.
        /// </summary>
        public int RollsMade { get; set; }

        /// <summary>
        /// Gets the players in the order of the turns.
        /// </summary>
        public List<SavedPlayer> Players { get; } = new List<SavedPlayer>();

        /// <summary>
        /// Gets the values of the dice of the current turn, or an empty list if the player did not roll yet.
        /// </summary>
        public List<int> Dice { get; } = new List<int>();

        /// <summary>
        /// Gets which dice the player kept for the next roll, in the same order as the dice.
        /// </summary>
        public List<bool> HeldDice { get; } = new List<bool>();
    }
}
