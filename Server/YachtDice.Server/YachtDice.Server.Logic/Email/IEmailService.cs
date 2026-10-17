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
        /// <param name="to">Recipient email address.</param>
        /// <param name="subject">Subject of the email.</param>
        /// <param name="body">Body of the email.</param>
        void Send(string to, string subject, string body);
    }
}