using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// Rule that decides when the Yacht celebration is shown.
    /// </summary>
    public class YachtCelebrationRuleTests
    {
        private const int YachtPoints = 50;
        private const int NoPoints = 0;

        [Fact]
        public void IsCelebrationDue_YachtBoxJustFilledWithFifty_ReturnsTrue()
        {
            var isDue = YachtCelebrationRule.IsCelebrationDue(false, true, YachtPoints);

            Assert.True(isDue);
        }

        [Fact]
        public void IsCelebrationDue_YachtBoxJustFilledWithZero_ReturnsFalse()
        {
            var isDue = YachtCelebrationRule.IsCelebrationDue(false, true, NoPoints);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_YachtBoxAlreadyFilled_ReturnsFalse()
        {
            var isDue = YachtCelebrationRule.IsCelebrationDue(true, true, YachtPoints);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_YachtBoxStillOpen_ReturnsFalse()
        {
            var isDue = YachtCelebrationRule.IsCelebrationDue(false, false, NoPoints);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_FortyNinePoints_ReturnsFalse()
        {
            bool isDue = YachtCelebrationRule.IsCelebrationDue(false, true, YachtPoints - 1);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_FiftyOnePoints_ReturnsFalse()
        {
            bool isDue = YachtCelebrationRule.IsCelebrationDue(false, true, YachtPoints + 1);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_BoxReopenedWithFifty_ReturnsFalse()
        {
            bool isDue = YachtCelebrationRule.IsCelebrationDue(true, false, YachtPoints);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_NegativePoints_ReturnsFalse()
        {
            bool isDue = YachtCelebrationRule.IsCelebrationDue(false, true, -YachtPoints);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_BoxStillOpenWithFifty_ReturnsFalse()
        {
            bool isDue = YachtCelebrationRule.IsCelebrationDue(false, false, YachtPoints);

            Assert.False(isDue);
        }
    }
}
