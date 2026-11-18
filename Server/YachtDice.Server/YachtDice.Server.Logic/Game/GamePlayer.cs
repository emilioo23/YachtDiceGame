using System;
namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// The state of a player inside a game: the name, the scorecard and whether the player is connected.
    /// </summary>
    internal sealed class GamePlayer
    {
        private const string NullParticipantMessage = "The participant of the game cannot be null.";
        private const string NullCardMessage = "The scorecard of the player cannot be null.";

        /// <summary>
        /// Initializes a player who starts present and with an empty scorecard.
        /// </summary>
        /// <param name="participant">Player who takes part in the game.</param>
        /// <param name="card">Scorecard of the player.</param>
        public GamePlayer(GameParticipant participant, ScoreCard card)
        {
            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant), NullParticipantMessage);
            }

            PlayerId = participant.PlayerId;
            Name = participant.Name;
            IsComputer = participant.IsComputer;
            Skill = ComputerSkill.For(participant.Difficulty);
            Card = card ?? throw new ArgumentNullException(nameof(card), NullCardMessage);
            IsPresent = true;
        }

        /// <summary>
        /// Gets the identifier of the player.
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// Gets the name that the screens show for the player.
        /// </summary>
        public string Name { get; }

        public bool IsComputer { get; }

        public ComputerSkill Skill { get; }

        /// <summary>
        /// Gets the scorecard of the player.
        /// </summary>
        public ScoreCard Card { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the player is connected to the game.
        /// </summary>
        public bool IsPresent { get; set; }

        public bool HasForfeited { get; set; }

        /// <summary>
        /// Gets a value indicating whether the player can play a turn.
        /// </summary>
        public bool CanPlay => IsPresent && !Card.IsComplete;
    }
}
