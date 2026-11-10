using Xunit;
using YachtDice.Client.Logic.Players;

namespace YachtDice.Client.Tests.Players
{
    /// <summary>
    /// Rules to choose the initial that is shown for a player.
    /// </summary>
    public class InitialLetterTests
    {
        private const string Unknown = "?";

        [Fact]
        public void From_LowercaseName_ReturnsTheFirstLetterInUppercase()
        {
            Assert.Equal("E", InitialLetter.From("emilio", Unknown));
        }

        [Fact]
        public void From_NameWithSpacesAround_IgnoresTheSpaces()
        {
            Assert.Equal("G", InitialLetter.From("   gabriel", Unknown));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void From_BlankName_ReturnsTheUnknownInitial(string name)
        {
            Assert.Equal(Unknown, InitialLetter.From(name, Unknown));
        }

        [Fact]
        public void From_NameThatStartsWithANumber_ReturnsThatNumber()
        {
            Assert.Equal("7", InitialLetter.From("7seas", Unknown));
        }
    }
}
