namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Reasons that a player can choose to report another player.
    /// </summary>
    public enum ReportReason
    {
        /// <summary>The player used insults or offensive words.</summary>
        OffensiveLanguage,

        /// <summary>The player seemed to cheat or use the game in a way it is not meant to be used.</summary>
        Cheating,

        /// <summary>The player behaved badly on purpose, for example making the others wait.</summary>
        UnsportingBehavior,

        /// <summary>Any other reason, explained in the comment.</summary>
        Other
    }
}
