using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// Rule that decides when the Yacht celebration is shown.
    /// </summary>
    public class YachtCelebrationRuleTests
    {
        [Fact]
        public void IsCelebrationDue_YachtBoxJustFilledWithFifty_ReturnsTrue()
        {
            var isDue = YachtCelebrationRule.IsCelebrationDue(false, true, 50);

            Assert.True(isDue);
        }

        [Fact]
        public void IsCelebrationDue_YachtBoxJustFilledWithZero_ReturnsFalse()
        {
            var isDue = YachtCelebrationRule.IsCelebrationDue(false, true, 0);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_YachtBoxAlreadyFilled_ReturnsFalse()
        {
            var isDue = YachtCelebrationRule.IsCelebrationDue(true, true, 50);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_YachtBoxStillOpen_ReturnsFalse()
        {
            var isDue = YachtCelebrationRule.IsCelebrationDue(false, false, 0);

            Assert.False(isDue);
        }
    }
}
