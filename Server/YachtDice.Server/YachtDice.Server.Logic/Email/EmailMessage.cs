namespace YachtDice.Server.Logic.Email
{
    /// <summary>
    /// A plain-text email ready to be sent.
    /// </summary>
    public sealed class EmailMessage
    {
        /// <summary>
        /// Gets or sets the email address of the recipient.
        /// </summary>
        public string ToAddress { get; set; }

        /// <summary>
        /// Gets or sets the subject of the email.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the plain-text body of the email.
        /// </summary>
        public string Body { get; set; }
    }
}
