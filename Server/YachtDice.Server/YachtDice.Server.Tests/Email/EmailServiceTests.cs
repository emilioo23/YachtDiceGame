using System;
using Xunit;
using YachtDice.Server.Logic.Email;

namespace YachtDice.Server.Tests.Email
{
    /// <summary>
    /// Checks that the email service rejects the emails that cannot be sent.
    /// </summary>
    public class EmailServiceTests
    {
        private const string AnySubject = "Subject";
        private const string AnyBody = "Body";

        private readonly EmailService _service = new EmailService();

        [Fact]
        public void Send_NullMessage_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.Send(null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Send_EmptyRecipient_ThrowsArgumentException(string toAddress)
        {
            var message = new EmailMessage { ToAddress = toAddress, Subject = AnySubject, Body = AnyBody };

            Assert.Throws<ArgumentException>(() => _service.Send(message));
        }
    }
}
