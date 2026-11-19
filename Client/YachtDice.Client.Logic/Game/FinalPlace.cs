using System;
namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// A player and the points that the player has at the end of a game.
    /// </summary>
    public sealed class FinalPlace
    {
        private const string NullPlayerIdMessage = "The identifier of the player cannot be null.";
        private const string NullNameMessage = "The name of the player cannot be null.";

        /// <summary>
        /// Initializes a new entry.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="name">Name that the screens show for the player.</param>
        /// <param name="total">Total points of the player.</param>
        public FinalPlace(string playerId, string name, int total)
        {
            PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId), NullPlayerIdMessage);
            Name = name ?? throw new ArgumentNullException(nameof(name), NullNameMessage);
            Total = total;
        }

        /// <summary>
        /// Gets the identifier of the player.
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// Gets the name that the screens show for the player.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the total points of the player.
        /// </summary>
        public int Total { get; }

        /// <summary>
        /// Gets or sets the experience that the player earned in the game.
        /// </summary>
        public int ExperienceGained { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player has an account. Only a player with an account can
        /// be reported.
        /// </summary>
        public bool IsRegistered { get; set; }

        /// <summary>
        /// Gets or sets the points of the upper section, before its bonus.
        /// </summary>
        public int UpperSubtotal { get; set; }

        /// <summary>
        /// Gets or sets the bonus of the upper section.
        /// </summary>
        public int UpperBonus { get; set; }

        /// <summary>
        /// Gets or sets the bonus of the extra Yachts.
        /// </summary>
        public int YachtBonus { get; set; }

        /// <summary>
        /// Gets or sets the place of the player in the final scoreboard. Players with the same total share it.
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Gets the points of the lower section, without the upper section and the bonuses.
        /// </summary>
        public int LowerTotal => Total - UpperSubtotal - UpperBonus - YachtBonus;

        /// <summary>
        /// Gets a value indicating whether the player finished in the first place.
        /// </summary>
        public bool IsWinner => Rank == StandingsBuilder.FirstRank;
    }
}
