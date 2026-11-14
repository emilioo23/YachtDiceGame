using System.Collections.Generic;
using Xunit;
using YachtDice.Client.Logic.Players;

namespace YachtDice.Client.Tests.Players
{
    /// <summary>
    /// Detection of the achievements that a player just unlocked.
    /// </summary>
    public class AchievementChangesTests
    {
        private const int FirstGameId = 1;
        private const int FirstWinId = 2;
        private const int ThirdWinId = 3;

        [Fact]
        public void FindNewlyUnlocked_OneMoreAchievement_ReturnsOnlyTheNewOne()
        {
            var knownIds = new List<int> { FirstGameId };
            var currentIds = new List<int> { FirstGameId, FirstWinId };
            var expectedIds = new List<int> { FirstWinId };

            List<int> newIds = AchievementChanges.FindNewlyUnlocked(knownIds, currentIds);

            Assert.Equal(expectedIds, newIds);
        }

        [Fact]
        public void FindNewlyUnlocked_NothingNew_ReturnsAnEmptyList()
        {
            var knownIds = new List<int> { FirstGameId, FirstWinId };

            List<int> newIds = AchievementChanges.FindNewlyUnlocked(knownIds, knownIds);

            Assert.Empty(newIds);
        }

        [Fact]
        public void FindNewlyUnlocked_SeveralNewAchievements_ReturnsAllOfThem()
        {
            var knownIds = new List<int>();
            var currentIds = new List<int> { FirstGameId, FirstWinId, ThirdWinId };

            List<int> newIds = AchievementChanges.FindNewlyUnlocked(knownIds, currentIds);

            Assert.Equal(currentIds, newIds);
        }
    }
}
