using Xunit;
using YachtDice.Client.Logic.Settings;

namespace YachtDice.Client.Tests.Settings
{
    /// <summary>
    /// What the player chose about the help that the game gives while playing.
    /// </summary>
    public class GameplaySettingsTests
    {
        [Fact]
        public void New_Default_ShowsThePotentialPoints()
        {
            var settings = new GameplaySettings();

            bool showPotentialPointsResult = settings.ShowsPotentialPoints;

            Assert.True(showPotentialPointsResult);
        }

        [Fact]
        public void SetShowPotentialPoints_False_HidesThePotentialPoints()
        {
            var settings = new GameplaySettings();

            settings.SetShowPotentialPoints(false);

            bool showPotentialPointsResult = settings.ShowsPotentialPoints;

            Assert.False(showPotentialPointsResult);
        }

        [Fact]
        public void SetShowPotentialPoints_TrueAfterFalse_ShowsThePotentialPointsAgain()
        {
            var settings = new GameplaySettings();

            settings.SetShowPotentialPoints(false);
            settings.SetShowPotentialPoints(true);

            bool showPotentialPointsResult = settings.ShowsPotentialPoints;

            Assert.True(showPotentialPointsResult);
        }
    }
}
