namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Outcome of a report about another player.
    /// </summary>
    public enum ReportStatus
    {
        /// <summary>The report was saved.</summary>
        Sent,

        /// <summary>
        /// The report was saved and it was the one that suspends the reported player, because several different
        /// players reported them in the last day.
        /// </summary>
        SentAndSuspended,

        /// <summary>One of the two players does not have an account.</summary>
        PlayerNotFound,

        /// <summary>The player tried to report themselves.</summary>
        CannotReportSelf,

        /// <summary>The two players neither finished a game together recently nor play one now.</summary>
        NoRecentGame,

        /// <summary>The player already reported the same player recently.</summary>
        AlreadyReported,

        /// <summary>The comment is too long.</summary>
        InvalidComment
    }
}
