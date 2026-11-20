namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// What a player says about another player when reporting them.
    /// </summary>
    public class ReportDetails
    {
        /// <summary>
        /// Gets or sets the name of the player who is reported.
        /// </summary>
        public string ReportedName { get; set; }

        /// <summary>
        /// Gets or sets the reason chosen by the player who reports.
        /// </summary>
        public ReportReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the optional comment that explains what happened.
        /// </summary>
        public string Comment { get; set; }
    }
}
