using Xunit;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Letter shown in the avatar of a player.
    /// </summary>
    public class PlayerInitialsTests
    {
        private const string LowerCaseName = "ana";
        private const string UpperCaseInitial = "A";
        private const string BlankName = "   ";
        private const string NameWithSpaces = "   ana";
        private const string AccentedName = "álvaro";
        private const string AccentedInitial = "Á";
        private const string SingleLetterName = "x";
        private const string SingleLetterInitial = "X";

        [Fact]
        public void GetInitial_LowerCaseName_ReturnsTheFirstLetterInUpperCase()
        {
            string initial = PlayerInitials.GetInitial(LowerCaseName);

            Assert.Equal(UpperCaseInitial, initial);
        }

        [Fact]
        public void GetInitial_BlankName_ReturnsTheUnknownInitial()
        {
            string initial = PlayerInitials.GetInitial(BlankName);

            Assert.Equal(Strings.Global_UnknownInitial, initial);
        }

        [Fact]
        public void GetInitial_EmptyName_ReturnsTheUnknownInitial()
        {
            string initial = PlayerInitials.GetInitial(string.Empty);

            Assert.Equal(Strings.Global_UnknownInitial, initial);
        }

        [Fact]
        public void GetInitial_NullName_ReturnsTheUnknownInitial()
        {
            string initial = PlayerInitials.GetInitial(null);

            Assert.Equal(Strings.Global_UnknownInitial, initial);
        }

        [Fact]
        public void GetInitial_NameWithSpacesAround_IgnoresTheSpaces()
        {
            string initial = PlayerInitials.GetInitial(NameWithSpaces);

            Assert.Equal(UpperCaseInitial, initial);
        }

        [Fact]
        public void GetInitial_NameWithAnAccent_KeepsTheAccentInUpperCase()
        {
            string initial = PlayerInitials.GetInitial(AccentedName);

            Assert.Equal(AccentedInitial, initial);
        }

        [Fact]
        public void GetInitial_NameOfOneLetter_ReturnsThatLetterInUpperCase()
        {
            string initial = PlayerInitials.GetInitial(SingleLetterName);

            Assert.Equal(SingleLetterInitial, initial);
        }
    }
}
