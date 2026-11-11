using Xunit;
using YachtDice.GameServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Names of the boxes of the scorecard.
    /// </summary>
    public class CategoryNamesTests
    {

        [Fact]
        public void GetName_Yacht_ReturnsTheYachtName()
        {
            string message = CategoryNames.GetName(ScoreCategory.Yacht);

            Assert.Equal(Strings.Game_Category_Yacht, message);
        }

        [Fact]
        public void GetName_FullHouse_ReturnsTheFullHouseName()
        {
            string message = CategoryNames.GetName(ScoreCategory.FullHouse);

            Assert.Equal(Strings.Game_Category_FullHouse, message);
        }
    }
}
