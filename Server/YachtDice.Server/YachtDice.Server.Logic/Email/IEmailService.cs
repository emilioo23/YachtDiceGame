namespace YachtDice.Server.Logic.Email
{
    /// <summary>
    /// Contract for the email sending service.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email synchronously.
        /// </summary>
        /// <param name="message">Email to send, with its recipient, subject and body.</param>
        void Send(EmailMessage message);
    }
}