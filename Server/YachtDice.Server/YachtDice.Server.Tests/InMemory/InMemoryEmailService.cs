using System.Collections.Generic;
using YachtDice.Server.Logic.Email;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Remembers the recipients of the emails instead of sending them, so the tests do not need the network.
    /// </summary>
    internal sealed class InMemoryEmailService : IEmailService
    {
        private readonly List<string> _recipients = new List<string>();

        /// <summary>
        /// Gets the addresses that received an email, in order.
        /// </summary>
        public IReadOnlyList<string> Recipients => _recipients;

        /// <inheritdoc />
        public void Send(string toAddress, string subject, string body)
        {
            _recipients.Add(toAddress);
        }
    }
}
