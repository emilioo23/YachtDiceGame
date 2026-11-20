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

            bool showsPotentialPointsResult = settings.ShowsPotentialPoints;

            Assert.True(showsPotentialPointsResult);
        }

        [Fact]
        public void SetShowPotentialPoints_False_HidesThePotentialPoints()
        {
            var settings = new GameplaySettings();

            settings.SetShowPotentialPoints(false);

            bool showsPotentialPointsResult = settings.ShowsPotentialPoints;

            Assert.False(showsPotentialPointsResult);
        }

        [Fact]
        public void SetShowPotentialPoints_TrueAfterFalse_ShowsThePotentialPointsAgain()
        {
            var settings = new GameplaySettings();

            settings.SetShowPotentialPoints(false);
            settings.SetShowPotentialPoints(true);

            bool showsPotentialPointsResult = settings.ShowsPotentialPoints;

            Assert.True(showsPotentialPointsResult);
        }
    }
}
