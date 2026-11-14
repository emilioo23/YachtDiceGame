using System;
using Xunit;
using YachtDice.Server.Logic.Moderation;

namespace YachtDice.Server.Tests.Moderation
{
    /// <summary>
    /// Offensive words inside usernames and chat messages.
    /// </summary>
    public class OffensiveWordFilterTests
    {
        private const string ShortWord = "puta";
        private const string LongWord = "pendejo";
        private const string EnglishWord = "shit";
        private const string CleanUsername = "gabriel_99";
        private const string UsernameWithLongWord = "el.PENDEJO22";
        private const string UsernameWithNumbersAsLetters = "p3nd3j0";
        private const string UsernameWithShortWordAsPart = "puta_123";
        private const string UsernameWithShortWordInsideAnotherWord = "computadora";
        private const string MessageWithWords = "Eres un pendejo, shit!";
        private const string CensoredMessage = "Eres un *******, ****!";
        private const string MessageWithWordInsideAnotherWord = "La computadora se trabo";

        private const string BlankLine = " ";

        private static readonly string[] OffensiveWords = { ShortWord, LongWord, EnglishWord, BlankLine };

        private readonly OffensiveWordFilter _filter = new OffensiveWordFilter(OffensiveWords);

        [Fact]
        public void ContainsOffensiveWord_CleanUsername_ReturnsFalse()
        {
            bool isOffensive = _filter.ContainsOffensiveWord(CleanUsername);

            Assert.False(isOffensive);
        }

        [Theory]
        [InlineData(UsernameWithLongWord)]
        [InlineData(UsernameWithNumbersAsLetters)]
        [InlineData(UsernameWithShortWordAsPart)]
        public void ContainsOffensiveWord_UsernameWithOffensiveWord_ReturnsTrue(string username)
        {
            bool isOffensive = _filter.ContainsOffensiveWord(username);

            Assert.True(isOffensive);
        }

        [Fact]
        public void ContainsOffensiveWord_ShortWordInsideAnotherWord_ReturnsFalse()
        {
            bool isOffensive = _filter.ContainsOffensiveWord(UsernameWithShortWordInsideAnotherWord);

            Assert.False(isOffensive);
        }

        [Fact]
        public void Censor_MessageWithOffensiveWords_ReplacesEachLetterWithAnAsterisk()
        {
            string censoredMessage = _filter.Censor(MessageWithWords);

            Assert.Equal(CensoredMessage, censoredMessage);
        }

        [Fact]
        public void Censor_WordInsideAnotherWord_KeepsTheMessage()
        {
            string censoredMessage = _filter.Censor(MessageWithWordInsideAnotherWord);

            Assert.Equal(MessageWithWordInsideAnotherWord, censoredMessage);
        }

        [Fact]
        public void Censor_EmptyList_KeepsTheMessage()
        {
            var emptyFilter = new OffensiveWordFilter(Array.Empty<string>());

            string censoredMessage = emptyFilter.Censor(MessageWithWords);

            Assert.Equal(MessageWithWords, censoredMessage);
        }

        [Fact]
        public void Constructor_NullWords_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OffensiveWordFilter(null));
        }

        [Fact]
        public void ContainsOffensiveWord_NullUsername_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _filter.ContainsOffensiveWord(null));
        }
    }
}
