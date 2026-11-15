using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// A player who takes part in a game.
    /// </summary>
    public sealed class GameParticipant
    {
        /// <summary>
        /// Initializes a new participant.
        /// </summary>
        /// <param name="playerId">Identifier that the server gave to the player in the room.</param>
        /// <param name="name">Name that the screens show for the player.</param>
        public GameParticipant(string playerId, string name)
        {
            PlayerId = playerId;
            Name = name;
        }

        /// <summary>
        /// Gets the identifier that the server gave to the player in the room.
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// Gets the name that the screens show for the player.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the server plays the turns of this participant.
        /// </summary>
        public bool IsComputer { get; set; }

        /// <summary>
        /// Gets or sets how well the server plays the turns of this participant when it is a computer player.
        /// </summary>
        public ComputerDifficulty Difficulty { get; set; } = ComputerDifficulty.Normal;
    }
}
