namespace YachtDice.Utils
{
    /// <summary>
    /// Holds the texts that a dialog shows.
    /// </summary>
    public class DialogContent
    {
        /// <summary>
        /// Gets or sets the title of the dialog.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the main message of the dialog.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the text of the primary button. If it is empty, the button keeps its default text.
        /// </summary>
        public string PrimaryButtonText { get; set; }

        /// <summary>
        /// Gets or sets the text of the secondary button. If it is empty, the button is not shown.
        /// </summary>
        public string SecondaryButtonText { get; set; }
    }
}