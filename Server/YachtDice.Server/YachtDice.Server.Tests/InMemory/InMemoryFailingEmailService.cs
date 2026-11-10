using System;
using YachtDice.Server.Logic.Email;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Email service that always fails, to test what happens when the email cannot be delivered.
    /// </summary>
    internal sealed class InMemoryFailingEmailService : IEmailService
    {
        private const string FailureMessage = "The email could not be sent.";

        /// <inheritdoc />
        public void Send(string toAddress, string subject, string body)
        {
            throw new InvalidOperationException(FailureMessage);
        }
    }
}
