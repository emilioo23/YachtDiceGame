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
        private const string AccentedName = "álvaro";
        private const string AccentedInitial = "Á";
        private const string LowercaseName = "emilio";
        private const string LowercaseInitial = "E";
        private const string SpacedName = "   gabriel";
        private const string SpacedInitial = "G";
        private const string NumberName = "7seas";
        private const string NumberInitial = "7";

        [Fact]
        public void From_LowercaseName_ReturnsTheFirstLetterInUppercase()
        {
            Assert.Equal(LowercaseInitial, InitialLetter.From(LowercaseName, Unknown));
        }

        [Fact]
        public void From_NameWithSpacesAround_IgnoresTheSpaces()
        {
            Assert.Equal(SpacedInitial, InitialLetter.From(SpacedName, Unknown));
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
            Assert.Equal(NumberInitial, InitialLetter.From(NumberName, Unknown));
        }

        [Fact]
        public void From_NameWithAnAccent_KeepsTheAccentInUppercase()
        {
            Assert.Equal(AccentedInitial, InitialLetter.From(AccentedName, Unknown));
        }
    }
}
