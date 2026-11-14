using Xunit;
using YachtDice.Client.Logic.Chat;

namespace YachtDice.Client.Tests.Chat
{
    /// <summary>
    /// Chat messages that the window does not send because they have a link.
    /// </summary>
    public class ChatLinkDetectorTests
    {
        private const string FullLink = "mira https://spam.example.com";
        private const string WwwLink = "entra a www.spam.mx";
        private const string ShortLink = "mipagina.com";
        private const string MessageWithoutLinks = "Buena tirada. Te toca.";
        private const string EmptyMessage = "";

        [Theory]
        [InlineData(FullLink)]
        [InlineData(WwwLink)]
        [InlineData(ShortLink)]
        public void ContainsLink_MessageWithLink_ReturnsTrue(string message)
        {
            bool hasLink = ChatLinkDetector.ContainsLink(message);

            Assert.True(hasLink);
        }

        [Theory]
        [InlineData(MessageWithoutLinks)]
        [InlineData(EmptyMessage)]
        [InlineData(null)]
        public void ContainsLink_MessageWithoutLink_ReturnsFalse(string message)
        {
            bool hasLink = ChatLinkDetector.ContainsLink(message);

            Assert.False(hasLink);
        }
    }
}
