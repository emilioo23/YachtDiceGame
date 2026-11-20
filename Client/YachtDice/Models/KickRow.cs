namespace YachtDice.Models
{
    /// <summary>
    /// An expulsion as the history window shows it.
    /// </summary>
    public class KickRow
    {
        /// <summary>
        /// Gets or sets the sentence that says who expelled whom and from which room.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the date of the expulsion, already written in the format of the player.
        /// </summary>
        public string DateText { get; set; }
    }
}
