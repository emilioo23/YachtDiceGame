namespace YachtDice.Models
{
    /// <summary>
    /// One step of the turn, as the "How to play" window shows it.
    /// </summary>
    public class RuleStep
    {
        /// <summary>
        /// Gets or sets the number of the step, starting at 1.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets the text of the step.
        /// </summary>
        public string Text { get; set; }
    }
}
