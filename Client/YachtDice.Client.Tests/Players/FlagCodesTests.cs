using Xunit;
using YachtDice.Client.Logic.Players;

namespace YachtDice.Client.Tests.Players
{
    /// <summary>
    /// Countries whose flag a player can choose.
    /// </summary>
    public class FlagCodesTests
    {
        private const string MexicoCode = "mx";
        private const string SpainCode = "es";
        private const string UnknownCode = "zz";
        private const string UpperCaseCode = "MX";
        private const int CountryCount = 249;

        [Fact]
        public void GetAll_ReturnsEveryCountry()
        {
            Assert.Equal(CountryCount, FlagCodes.GetAll().Count);
        }

        [Fact]
        public void IsKnown_CountryOfTheList_ReturnsTrue()
        {
            Assert.True(FlagCodes.IsKnown(MexicoCode));
        }

        [Fact]
        public void IsKnown_OtherCountryOfTheList_ReturnsTrue()
        {
            Assert.True(FlagCodes.IsKnown(SpainCode));
        }

        [Fact]
        public void IsKnown_CodeThatDoesNotExist_ReturnsFalse()
        {
            Assert.False(FlagCodes.IsKnown(UnknownCode));
        }

        [Fact]
        public void IsKnown_NullCode_ReturnsFalse()
        {
            Assert.False(FlagCodes.IsKnown(null));
        }

        [Fact]
        public void IsKnown_CodeInUpperCase_ReturnsFalse()
        {
            Assert.False(FlagCodes.IsKnown(UpperCaseCode));
        }
    }
}
