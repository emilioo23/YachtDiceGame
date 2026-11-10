using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Game;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// Scorecard of a player: open boxes, bonuses and the Joker rule.
    /// </summary>
    public class ScoreCardTests
    {
        private static readonly int[] YachtOfFours = { 4, 4, 4, 4, 4 };
        private static readonly int[] YachtOfSixes = { 6, 6, 6, 6, 6 };
        private static readonly int[] NoCombination = { 1, 2, 3, 4, 6 };

        [Fact]
        public void Score_OpenCategory_WritesThePointsAndAddsThemToTheTotal()
        {
            var card = CreateCard();

            var status = card.Score(ScoreCategory.Choice, NoCombination);

            Assert.Equal(ScoreStatus.Success, status);
            Assert.Equal(16, card.GetScore(ScoreCategory.Choice));
            Assert.Equal(16, card.Total);
        }

        [Fact]
        public void Score_CategoryAlreadyFilled_ReturnsAlreadyFilledAndKeepsThePoints()
        {
            var card = CreateCard();
            card.Score(ScoreCategory.Choice, NoCombination);

            var status = card.Score(ScoreCategory.Choice, YachtOfSixes);

            Assert.Equal(ScoreStatus.AlreadyFilled, status);
            Assert.Equal(16, card.GetScore(ScoreCategory.Choice));
        }

        [Fact]
        public void GetScore_OpenCategory_ReturnsZero()
        {
            var card = CreateCard();

            Assert.Equal(0, card.GetScore(ScoreCategory.Sixes));
            Assert.False(card.IsFilled(ScoreCategory.Sixes));
        }

        [Fact]
        public void Score_ZeroPoints_StillFillsTheCategory()
        {
            var card = CreateCard();

            card.Score(ScoreCategory.Yacht, NoCombination);

            Assert.True(card.IsFilled(ScoreCategory.Yacht));
            Assert.Equal(0, card.GetScore(ScoreCategory.Yacht));
        }

        [Fact]
        public void UpperBonus_UpperSectionReachesSixtyThree_AddsThirtyFivePoints()
        {
            var card = CreateCard();
            ScoreThreeOfEachUpperValue(card);

            Assert.Equal(63, card.UpperSubtotal);
            Assert.Equal(35, card.UpperBonus);
            Assert.Equal(98, card.Total);
        }

        [Fact]
        public void UpperBonus_UpperSectionBelowSixtyThree_IsZero()
        {
            var fewerSixes = CreateCard();
            ScoreUpperWithTwoSixes(fewerSixes);

            Assert.Equal(57, fewerSixes.UpperSubtotal);
            Assert.Equal(0, fewerSixes.UpperBonus);
        }

        [Fact]
        public void YachtBonus_SecondYachtAfterFiftyPoints_AddsOneHundred()
        {
            var card = CreateCard();
            card.Score(ScoreCategory.Yacht, YachtOfFours);

            card.Score(ScoreCategory.Sixes, YachtOfSixes);

            Assert.Equal(100, card.YachtBonus);
            Assert.Equal(30, card.GetScore(ScoreCategory.Sixes));
            Assert.Equal(180, card.Total);
        }

        [Fact]
        public void YachtBonus_YachtBoxWithZeroPoints_DoesNotAddTheBonus()
        {
            var card = CreateCard();
            card.Score(ScoreCategory.Yacht, NoCombination);

            card.Score(ScoreCategory.Sixes, YachtOfSixes);

            Assert.Equal(0, card.YachtBonus);
        }

        [Fact]
        public void YachtBonus_YachtWhileTheYachtBoxIsOpen_DoesNotAddTheBonus()
        {
            var card = CreateCard();

            card.Score(ScoreCategory.Choice, YachtOfSixes);

            Assert.Equal(0, card.YachtBonus);
        }

        [Fact]
        public void GetOptions_NoYacht_ReturnsEveryOpenCategory()
        {
            var card = CreateCard();
            card.Score(ScoreCategory.Choice, NoCombination);

            var options = card.GetOptions(NoCombination);

            Assert.Equal(11, options.Count);
            Assert.False(options.Any(option => option.Category == ScoreCategory.Choice));
        }

        [Fact]
        public void GetOptions_JokerWithMatchingUpperBoxOpen_ReturnsOnlyThatBox()
        {
            var card = CreateCard();
            card.Score(ScoreCategory.Yacht, YachtOfSixes);

            var options = card.GetOptions(YachtOfFours);

            Assert.Equal(1, options.Count);
            Assert.Equal(ScoreCategory.Fours, options.Single().Category);
            Assert.Equal(20, options.Single().Points);
        }

        [Fact]
        public void GetOptions_JokerWithMatchingUpperBoxFilled_ReturnsOnlyOpenLowerBoxes()
        {
            var card = CreateCard();
            card.Score(ScoreCategory.Yacht, YachtOfSixes);
            card.Score(ScoreCategory.Fours, NoCombination);

            var options = card.GetOptions(YachtOfFours);

            Assert.False(options.Any(option => ScoreCategories.IsUpper(option.Category)));
            Assert.Equal(5, options.Count);
            Assert.Equal(30, options.Single(option => option.Category == ScoreCategory.BigStraight).Points);
            Assert.Equal(20, options.Single(option => option.Category == ScoreCategory.Choice).Points);
        }

        [Fact]
        public void GetOptions_JokerWithLowerSectionFull_ReturnsOpenUpperBoxesWithZeroPoints()
        {
            var card = CreateCard();
            FillLowerSection(card);
            card.Score(ScoreCategory.Fours, NoCombination);

            var options = card.GetOptions(YachtOfFours);

            Assert.Equal(5, options.Count);
            Assert.True(options.All(option => ScoreCategories.IsUpper(option.Category)));
            Assert.True(options.All(option => option.Points == 0));
        }

        [Fact]
        public void Score_JokerInABoxThatTheRuleDoesNotAllow_ReturnsNotAllowed()
        {
            var card = CreateCard();
            card.Score(ScoreCategory.Yacht, YachtOfSixes);

            var status = card.Score(ScoreCategory.Choice, YachtOfFours);

            Assert.Equal(ScoreStatus.NotAllowed, status);
            Assert.False(card.IsFilled(ScoreCategory.Choice));
        }

        [Fact]
        public void IsComplete_AllCategoriesFilled_ReturnsTrue()
        {
            var card = CreateCard();

            foreach (var category in ScoreCategories.All)
            {
                card.Score(category, NoCombination);
            }

            Assert.True(card.IsComplete);
            Assert.Equal(0, card.GetOpenCategories().Count);
        }

        [Fact]
        public void IsComplete_SomeCategoryOpen_ReturnsFalse()
        {
            var card = CreateCard();
            card.Score(ScoreCategory.Choice, NoCombination);

            Assert.False(card.IsComplete);
        }

        private static ScoreCard CreateCard()
        {
            return new ScoreCard(new ScoreCalculator());
        }

        private static void ScoreThreeOfEachUpperValue(ScoreCard card)
        {
            card.Score(ScoreCategory.Ones, new[] { 1, 1, 1, 2, 3 });
            card.Score(ScoreCategory.Twos, new[] { 2, 2, 2, 3, 4 });
            card.Score(ScoreCategory.Threes, new[] { 3, 3, 3, 1, 2 });
            card.Score(ScoreCategory.Fours, new[] { 4, 4, 4, 1, 2 });
            card.Score(ScoreCategory.Fives, new[] { 5, 5, 5, 1, 2 });
            card.Score(ScoreCategory.Sixes, new[] { 6, 6, 6, 1, 2 });
        }

        private static void ScoreUpperWithTwoSixes(ScoreCard card)
        {
            card.Score(ScoreCategory.Ones, new[] { 1, 1, 1, 2, 3 });
            card.Score(ScoreCategory.Twos, new[] { 2, 2, 2, 3, 4 });
            card.Score(ScoreCategory.Threes, new[] { 3, 3, 3, 1, 2 });
            card.Score(ScoreCategory.Fours, new[] { 4, 4, 4, 1, 2 });
            card.Score(ScoreCategory.Fives, new[] { 5, 5, 5, 1, 2 });
            card.Score(ScoreCategory.Sixes, new[] { 6, 6, 1, 2, 3 });
        }

        private static void FillLowerSection(ScoreCard card)
        {
            card.Score(ScoreCategory.Yacht, YachtOfSixes);
            card.Score(ScoreCategory.FullHouse, NoCombination);
            card.Score(ScoreCategory.FourOfAKind, NoCombination);
            card.Score(ScoreCategory.LittleStraight, NoCombination);
            card.Score(ScoreCategory.BigStraight, NoCombination);
            card.Score(ScoreCategory.Choice, NoCombination);
        }
    }
}
