using Xunit;
using YachtDice.Models;

namespace YachtDice.Tests
{
    public class ScoreValidatorTests
    {
        [Theory]
        [InlineData(new int[] { 1, 1, 2, 3, 4 }, ScoreCategory.Ones, 2)]
        [InlineData(new int[] { 2, 2, 2, 2, 5 }, ScoreCategory.FullHouse, 0)]
        [InlineData(new int[] { 2, 2, 3, 3, 3 }, ScoreCategory.FullHouse, 25)]
        [InlineData(new int[] { 1, 2, 3, 4, 1 }, ScoreCategory.SmallStraight, 30)]
        [InlineData(new int[] { 1, 2, 3, 4, 5 }, ScoreCategory.LargeStraight, 40)]
        [InlineData(new int[] { 6, 6, 6, 6, 6 }, ScoreCategory.Yacht, 50)]
        [InlineData(new int[] { 6, 6, 6, 6, 6 }, ScoreCategory.FourOfAKind, 30)]
        [InlineData(new int[] { 2, 3, 4, 5, 6 }, ScoreCategory.Choice, 20)]
        public void CalculateScore_ReturnsCorrectPoints(int[] dice, ScoreCategory category, int expectedScore)
        {
            int actualScore = ScoreValidator.CalculateScore(dice, category);
            Assert.Equal(expectedScore, actualScore);
        }
    }
}