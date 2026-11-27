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
        private const string PlainHttpLink = "http://juegos.example.org/sala";
        private const string UppercaseLink = "VISITA WWW.SPAM.COM";
        private const string GamingDomain = "unete a misala.gg";
        private const string SubdomainLink = "ve a partidas.yacht.io ahora";
        private const string EmailAddress = "escribeme a gabriel@gmail.com";
        private const string DecimalNumber = "saque 3.5 puntos de promedio";
        private const string SentencesWithDots = "Buena. Otra vez. Ya casi.";
        private const string UnknownEnding = "archivo.txt";
        private const string OnlySpaces = "   ";

        [Theory]
        [InlineData(FullLink)]
        [InlineData(WwwLink)]
        [InlineData(ShortLink)]
        [InlineData(PlainHttpLink)]
        [InlineData(UppercaseLink)]
        [InlineData(GamingDomain)]
        [InlineData(SubdomainLink)]
        [InlineData(EmailAddress)]
        public void ContainsLink_MessageWithLink_ReturnsTrue(string message)
        {
            bool hasLink = ChatLinkDetector.ContainsLink(message);

            Assert.True(hasLink);
        }

        [Theory]
        [InlineData(MessageWithoutLinks)]
        [InlineData(EmptyMessage)]
        [InlineData(null)]
        [InlineData(DecimalNumber)]
        [InlineData(SentencesWithDots)]
        [InlineData(UnknownEnding)]
        [InlineData(OnlySpaces)]
        public void ContainsLink_MessageWithoutLink_ReturnsFalse(string message)
        {
            bool hasLink = ChatLinkDetector.ContainsLink(message);

            Assert.False(hasLink);
        }
    }
}
