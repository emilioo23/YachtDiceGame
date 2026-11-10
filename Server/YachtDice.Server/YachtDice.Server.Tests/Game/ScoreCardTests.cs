using System.Collections.Generic;
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
        private const int NoCombinationChoicePoints = 16;
        private const int UpperBonusThreshold = 63;
        private const int UpperBonusPoints = 35;
        private const int TotalWithUpperBonus = 98;
        private const int UpperSubtotalWithTwoSixes = 57;
        private const int YachtBonusPoints = 100;
        private const int YachtOfSixesSixesPoints = 30;
        private const int TotalWithYachtBonus = 180;
        private const int OpenCategoriesAfterOneScore = 11;
        private const int YachtOfFoursPoints = 20;
        private const int BigStraightPoints = 30;
        private const int OpenLowerBoxesWithJoker = 5;
        private const int OpenUpperBoxesWithJoker = 5;

        private static readonly int[] YachtOfFours = { 4, 4, 4, 4, 4 };
        private static readonly int[] YachtOfSixes = { 6, 6, 6, 6, 6 };
        private static readonly int[] NoCombination = { 1, 2, 3, 4, 6 };
        private static readonly int[] ThreeOnes = { 1, 1, 1, 2, 3 };
        private static readonly int[] ThreeTwos = { 2, 2, 2, 3, 4 };
        private static readonly int[] ThreeThrees = { 3, 3, 3, 1, 2 };
        private static readonly int[] ThreeFours = { 4, 4, 4, 1, 2 };
        private static readonly int[] ThreeFives = { 5, 5, 5, 1, 2 };
        private static readonly int[] ThreeSixes = { 6, 6, 6, 1, 2 };
        private static readonly int[] TwoSixes = { 6, 6, 1, 2, 3 };

        [Fact]
        public void Score_OpenCategory_WritesThePointsAndAddsThemToTheTotal()
        {
            ScoreCard card = CreateCard();
            var expected = new
            {
                Status = ScoreStatus.Success,
                Points = NoCombinationChoicePoints,
                Total = NoCombinationChoicePoints
            };

            ScoreStatus status = card.Score(ScoreCategory.Choice, NoCombination);

            var actual = new { Status = status, Points = card.GetScore(ScoreCategory.Choice), card.Total };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Score_CategoryAlreadyFilled_ReturnsAlreadyFilledAndKeepsThePoints()
        {
            ScoreCard card = CreateCard();
            card.Score(ScoreCategory.Choice, NoCombination);
            var expected = new { Status = ScoreStatus.AlreadyFilled, Points = NoCombinationChoicePoints };

            ScoreStatus status = card.Score(ScoreCategory.Choice, YachtOfSixes);

            var actual = new { Status = status, Points = card.GetScore(ScoreCategory.Choice) };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetScore_OpenCategory_ReturnsZeroAndKeepsItOpen()
        {
            ScoreCard card = CreateCard();
            var expected = new { Points = 0, IsFilled = false };

            var actual = new
            {
                Points = card.GetScore(ScoreCategory.Sixes),
                IsFilled = card.IsFilled(ScoreCategory.Sixes)
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Score_ZeroPoints_StillFillsTheCategory()
        {
            ScoreCard card = CreateCard();
            var expected = new { IsFilled = true, Points = 0 };

            card.Score(ScoreCategory.Yacht, NoCombination);

            var actual = new
            {
                IsFilled = card.IsFilled(ScoreCategory.Yacht),
                Points = card.GetScore(ScoreCategory.Yacht)
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UpperBonus_UpperSectionReachesSixtyThree_AddsThirtyFivePoints()
        {
            ScoreCard card = CreateCard();
            var expected = new
            {
                UpperSubtotal = UpperBonusThreshold,
                UpperBonus = UpperBonusPoints,
                Total = TotalWithUpperBonus
            };

            ScoreThreeOfEachUpperValue(card);

            var actual = new { card.UpperSubtotal, card.UpperBonus, card.Total };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UpperBonus_UpperSectionBelowSixtyThree_IsZero()
        {
            ScoreCard fewerSixes = CreateCard();
            var expected = new { UpperSubtotal = UpperSubtotalWithTwoSixes, UpperBonus = 0 };

            ScoreUpperWithTwoSixes(fewerSixes);

            var actual = new { fewerSixes.UpperSubtotal, fewerSixes.UpperBonus };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void YachtBonus_SecondYachtAfterFiftyPoints_AddsOneHundred()
        {
            ScoreCard card = CreateCard();
            card.Score(ScoreCategory.Yacht, YachtOfFours);
            var expected = new
            {
                YachtBonus = YachtBonusPoints,
                SixesPoints = YachtOfSixesSixesPoints,
                Total = TotalWithYachtBonus
            };

            card.Score(ScoreCategory.Sixes, YachtOfSixes);

            var actual = new { card.YachtBonus, SixesPoints = card.GetScore(ScoreCategory.Sixes), card.Total };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void YachtBonus_YachtBoxWithZeroPoints_DoesNotAddTheBonus()
        {
            ScoreCard card = CreateCard();
            card.Score(ScoreCategory.Yacht, NoCombination);

            card.Score(ScoreCategory.Sixes, YachtOfSixes);

            Assert.Equal(0, card.YachtBonus);
        }

        [Fact]
        public void YachtBonus_YachtWhileTheYachtBoxIsOpen_DoesNotAddTheBonus()
        {
            ScoreCard card = CreateCard();

            card.Score(ScoreCategory.Choice, YachtOfSixes);

            Assert.Equal(0, card.YachtBonus);
        }

        [Fact]
        public void GetOptions_NoYacht_ReturnsEveryOpenCategory()
        {
            ScoreCard card = CreateCard();
            card.Score(ScoreCategory.Choice, NoCombination);
            var expected = new { OptionCount = OpenCategoriesAfterOneScore, OffersChoice = false };

            IReadOnlyList<ScoreOption> options = card.GetOptions(NoCombination);

            var actual = new
            {
                OptionCount = options.Count,
                OffersChoice = options.Any(option => option.Category == ScoreCategory.Choice)
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOptions_JokerWithMatchingUpperBoxOpen_ReturnsOnlyThatBox()
        {
            ScoreCard card = CreateCard();
            card.Score(ScoreCategory.Yacht, YachtOfSixes);
            var expected = new { Category = ScoreCategory.Fours, Points = YachtOfFoursPoints };

            IReadOnlyList<ScoreOption> options = card.GetOptions(YachtOfFours);

            ScoreOption option = options.Single();
            var actual = new { option.Category, option.Points };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOptions_JokerWithMatchingUpperBoxFilled_ReturnsOnlyOpenLowerBoxes()
        {
            ScoreCard card = CreateCard();
            card.Score(ScoreCategory.Yacht, YachtOfSixes);
            card.Score(ScoreCategory.Fours, NoCombination);
            var expected = new
            {
                OffersUpperBox = false,
                OptionCount = OpenLowerBoxesWithJoker,
                BigStraightPoints,
                ChoicePoints = YachtOfFoursPoints
            };

            IReadOnlyList<ScoreOption> options = card.GetOptions(YachtOfFours);

            var actual = new
            {
                OffersUpperBox = options.Any(option => ScoreCategories.IsUpper(option.Category)),
                OptionCount = options.Count,
                BigStraightPoints = FindOption(options, ScoreCategory.BigStraight).Points,
                ChoicePoints = FindOption(options, ScoreCategory.Choice).Points
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOptions_JokerWithLowerSectionFull_ReturnsOpenUpperBoxesWithZeroPoints()
        {
            ScoreCard card = CreateCard();
            FillLowerSection(card);
            card.Score(ScoreCategory.Fours, NoCombination);
            var expected = new { OptionCount = OpenUpperBoxesWithJoker, AreAllUpper = true, AreAllZero = true };

            IReadOnlyList<ScoreOption> options = card.GetOptions(YachtOfFours);

            var actual = new
            {
                OptionCount = options.Count,
                AreAllUpper = options.All(option => ScoreCategories.IsUpper(option.Category)),
                AreAllZero = options.All(option => option.Points == 0)
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Score_JokerInABoxThatTheRuleDoesNotAllow_ReturnsNotAllowedAndKeepsTheBoxOpen()
        {
            ScoreCard card = CreateCard();
            card.Score(ScoreCategory.Yacht, YachtOfSixes);
            var expected = new { Status = ScoreStatus.NotAllowed, IsFilled = false };

            ScoreStatus status = card.Score(ScoreCategory.Choice, YachtOfFours);

            var actual = new { Status = status, IsFilled = card.IsFilled(ScoreCategory.Choice) };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsComplete_AllCategoriesFilled_ReturnsTrueWithoutOpenCategories()
        {
            ScoreCard card = CreateCard();
            var expected = new { IsComplete = true, OpenCategoryCount = 0 };

            foreach (ScoreCategory category in ScoreCategories.All)
            {
                card.Score(category, NoCombination);
            }

            var actual = new { card.IsComplete, OpenCategoryCount = card.GetOpenCategories().Count };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsComplete_SomeCategoryOpen_ReturnsFalse()
        {
            ScoreCard card = CreateCard();

            card.Score(ScoreCategory.Choice, NoCombination);

            Assert.False(card.IsComplete);
        }

        private static ScoreCard CreateCard()
        {
            return new ScoreCard(new ScoreCalculator());
        }

        private static ScoreOption FindOption(IEnumerable<ScoreOption> options, ScoreCategory category)
        {
            return options.Single(option => option.Category == category);
        }

        private static void ScoreThreeOfEachUpperValue(ScoreCard card)
        {
            ScoreOnesToFives(card);
            card.Score(ScoreCategory.Sixes, ThreeSixes);
        }

        private static void ScoreUpperWithTwoSixes(ScoreCard card)
        {
            ScoreOnesToFives(card);
            card.Score(ScoreCategory.Sixes, TwoSixes);
        }

        private static void ScoreOnesToFives(ScoreCard card)
        {
            card.Score(ScoreCategory.Ones, ThreeOnes);
            card.Score(ScoreCategory.Twos, ThreeTwos);
            card.Score(ScoreCategory.Threes, ThreeThrees);
            card.Score(ScoreCategory.Fours, ThreeFours);
            card.Score(ScoreCategory.Fives, ThreeFives);
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
