namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Outcome of a report about another player.
    /// </summary>
    public enum ReportStatus
    {
        /// <summary>The report was saved.</summary>
        Sent,

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
