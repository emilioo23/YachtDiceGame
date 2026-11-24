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

        /// <summary>
        /// Gets or sets a value indicating whether both players are in the same game that is still in progress.
        /// The server decides it; the player who reports cannot say it. In that case the report does not need a
        /// finished game.
        /// </summary>
        public bool IsDuringGame { get; set; }
    }
}
