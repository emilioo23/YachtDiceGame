namespace YachtDice.Client.Logic.Friends
{
    /// <summary>
    /// Limits of a report about another player. They must match the limits of the server.
    /// </summary>
    public static class ReportLimits
    {
        /// <summary>
        /// Most characters that the comment of a report can have.
        /// </summary>
        public const int CommentMaxLength = 300;
    }
}
