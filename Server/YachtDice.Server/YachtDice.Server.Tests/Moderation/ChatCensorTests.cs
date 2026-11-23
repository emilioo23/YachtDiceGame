using System;
using Xunit;
using YachtDice.Server.Logic.Moderation;

namespace YachtDice.Server.Tests.Moderation
{
    /// <summary>
    /// Links hidden in the chat messages.
    /// </summary>
    public class ChatCensorTests
    {
        private const string MessageWithFullLink = "mira https://spam.example.com/x";
        private const string CensoredFullLink = "mira **************************";
        private const string MessageWithShortLink = "entra a mipagina.com ya";
        private const string CensoredShortLink = "entra a ************ ya";
        private const string MessageWithoutLinks = "Buena tirada. Te toca.";

        [Theory]
        [InlineData(MessageWithFullLink, CensoredFullLink)]
        [InlineData(MessageWithShortLink, CensoredShortLink)]
        public void Censor_MessageWithLink_ReplacesTheLinkWithAsterisks(string message, string messageExpected)
        {
            string censoredMessage = ChatCensor.Censor(message);

            Assert.Equal(messageExpected, censoredMessage);
        }

        [Fact]
        public void Censor_MessageWithoutLinks_KeepsTheMessage()
        {
            string censoredMessage = ChatCensor.Censor(MessageWithoutLinks);

            Assert.Equal(MessageWithoutLinks, censoredMessage);
        }

        [Fact]
        public void Censor_NullMessage_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ChatCensor.Censor(null));
        }
    }
}
