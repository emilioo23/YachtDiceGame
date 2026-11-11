namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Outcome of writing points in a box of the scorecard.
    /// </summary>
    public enum ScoreStatus
    {
        /// <summary>The points were written.</summary>
        Success,

        /// <summary>The box already has points.</summary>
        AlreadyFilled,

        /// <summary>The Joker rule does not allow to use the box with the current dice.</summary>
        NotAllowed
    }
}
