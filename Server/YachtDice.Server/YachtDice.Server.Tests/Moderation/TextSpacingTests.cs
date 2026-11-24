using System;
using Xunit;
using YachtDice.Server.Logic.Moderation;

namespace YachtDice.Server.Tests.Moderation
{
    /// <summary>
    /// Spaces that the players write together in the texts.
    /// </summary>
    public class TextSpacingTests
    {
        private const string TextWithManySpaces = "hola      a     todos";
        private const string TextWithTabsAndLineBreaks = "hola\t\ta\r\n\r\ntodos";
        private const string TextWithSurroundingSpaces = "   hola a todos   ";
        private const string TextWithSingleSpaces = "hola a todos";
        private const string OnlySpaces = "      ";

        [Theory]
        [InlineData(TextWithManySpaces)]
        [InlineData(TextWithTabsAndLineBreaks)]
        [InlineData(TextWithSurroundingSpaces)]
        [InlineData(TextWithSingleSpaces)]
        public void Collapse_TextWithSpaces_LeavesSingleSpacesBetweenTheWords(string text)
        {
            string collapsedText = TextSpacing.Collapse(text);

            Assert.Equal(TextWithSingleSpaces, collapsedText);
        }

        [Fact]
        public void Collapse_OnlySpaces_ReturnsAnEmptyText()
        {
            string collapsedText = TextSpacing.Collapse(OnlySpaces);

            Assert.Equal(string.Empty, collapsedText);
        }

        [Fact]
        public void Collapse_NullText_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => TextSpacing.Collapse(null));
        }
    }
}
