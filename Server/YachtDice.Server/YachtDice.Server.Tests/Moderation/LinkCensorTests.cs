using System;
using Xunit;
using YachtDice.Server.Logic.Moderation;

namespace YachtDice.Server.Tests.Moderation
{
    /// <summary>
    /// Links hidden in the chat messages.
    /// </summary>
    public class LinkCensorTests
    {
        private const string MessageWithFullLink = "mira https://spam.example.com/x";
        private const string CensoredFullLink = "mira **************************";
        private const string MessageWithShortLink = "entra a mipagina.com ya";
        private const string CensoredShortLink = "entra a ************ ya";
        private const string MessageWithoutLinks = "Buena tirada. Te toca.";

        [Theory]
        [InlineData(MessageWithFullLink, CensoredFullLink)]
        [InlineData(MessageWithShortLink, CensoredShortLink)]
        public void Censor_MessageWithLink_ReplacesTheLinkWithAsterisks(string message, string expectedMessage)
        {
            string censoredMessage = LinkCensor.Censor(message);

            Assert.Equal(expectedMessage, censoredMessage);
        }

        [Fact]
        public void Censor_MessageWithoutLinks_KeepsTheMessage()
        {
            string censoredMessage = LinkCensor.Censor(MessageWithoutLinks);

            Assert.Equal(MessageWithoutLinks, censoredMessage);
        }

        [Fact]
        public void Censor_NullMessage_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => LinkCensor.Censor(null));
        }
    }
}
