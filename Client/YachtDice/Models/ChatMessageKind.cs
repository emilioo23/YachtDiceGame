namespace YachtDice.Models
{
    /// <summary>
    /// Who wrote a message of the chat panel.
    /// </summary>
    public enum ChatMessageKind
    {
        /// <summary>A message written by another player.</summary>
        Other,

        /// <summary>A message written by the player who uses the window.</summary>
        Own,

        /// <summary>A notice of the game, not written by a player.</summary>
        System
    }
}
