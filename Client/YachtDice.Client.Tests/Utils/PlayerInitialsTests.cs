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
    }
}
