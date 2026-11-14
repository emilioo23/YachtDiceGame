using Xunit;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Names of the achievements and titles taken from their identifiers.
    /// </summary>
    public class ProgressTextsTests
    {
        private const int FirstWinAchievementId = 2;
        private const int DefaultTitleId = 0;
        private const int UnknownId = 9999;

        [Fact]
        public void GetAchievementName_ExistingAchievement_ReturnsItsName()
        {
            string name = ProgressTexts.GetAchievementName(FirstWinAchievementId);

            Assert.Equal(Strings.ResourceManager.GetString("Achievement_2_Name", Strings.Culture), name);
        }

        [Fact]
        public void GetAchievementName_UnknownAchievement_ReturnsAnEmptyText()
        {
            Assert.Equal(string.Empty, ProgressTexts.GetAchievementName(UnknownId));
        }

        [Fact]
        public void GetAchievementDescription_UnknownAchievement_ReturnsAnEmptyText()
        {
            Assert.Equal(string.Empty, ProgressTexts.GetAchievementDescription(UnknownId));
        }

        [Fact]
        public void GetTitleName_DefaultTitle_ReturnsTheInitialTitle()
        {
            string title = ProgressTexts.GetTitleName(DefaultTitleId);

            Assert.Equal(Strings.ResourceManager.GetString("Title_0", Strings.Culture), title);
        }

        [Fact]
        public void GetTitleName_UnknownTitle_ReturnsAnEmptyText()
        {
            Assert.Equal(string.Empty, ProgressTexts.GetTitleName(UnknownId));
        }
    }
}
