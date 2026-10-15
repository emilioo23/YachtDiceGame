using System.Threading.Tasks;

namespace YachtDice.Server.Logic.Email
{
    /// <summary>
    /// Sends emails to the players.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a plain-text email to the given recipient.
        /// </summary>
        /// <param name="toAddress">Recipient's email address.</param>
        /// <param name="subject">Subject of the email.</param>
        /// <param name="body">Plain-text body of the email.</param>
        /// <returns>A task that completes when the email has been sent.</returns>
        Task SendAsync(string toAddress, string subject, string body);
    }
}