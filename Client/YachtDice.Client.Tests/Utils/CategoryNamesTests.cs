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
        private const int UnknownCategoryValue = 99;
        private const string UnknownCategoryText = "99";

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

        [Fact]
        public void GetName_Ones_ReturnsOnesName()
        {
            string message = CategoryNames.GetName(ScoreCategory.Ones);

            Assert.Equal(Strings.Game_Category_Ones, message);
        }

        [Fact]
        public void GetName_Sixes_ReturnsSixesName()
        {
            string message = CategoryNames.GetName(ScoreCategory.Sixes);

            Assert.Equal(Strings.Game_Category_Sixes, message);
        }

        [Fact]
        public void GetName_LittleStraight_ReturnsLittleStraightName()
        {
            string message = CategoryNames.GetName(ScoreCategory.LittleStraight);

            Assert.Equal(Strings.Game_Category_LittleStraight, message);
        }

        [Fact]
        public void GetName_Choice_ReturnsChoiceName()
        {
            string message = CategoryNames.GetName(ScoreCategory.Choice);

            Assert.Equal(Strings.Game_Category_Choice, message);
        }

        [Fact]
        public void GetName_CategoryOutsideTheList_ReturnsTheNumberOfTheCategory()
        {
            string message = CategoryNames.GetName((ScoreCategory)UnknownCategoryValue);

            Assert.Equal(UnknownCategoryText, message);
        }
    }
}
