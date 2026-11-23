using System;
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
        private const int NoPoints = 0;
        private const int NoOpenCategories = 0;
        private const int UpperBonusThreshold = 63;
        private const int UpperBonusPoints = 35;
        private const int TotalWithUpperBonus = 98;
        private const int UpperSubtotalWithTwoSixes = 57;
        private const int YachtBonusPoints = 100;
        private const int YachtPoints = 50;
        private const int OneYachtBonus = 1;
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
            var scoreExpected = new
            {
                Status = GameActionStatus.Success,
                Points = NoCombinationChoicePoints,
                Total = NoCombinationChoicePoints
            };

            GameActionStatus status = card.Score(ScoreCategory.Choice, NoCombination);

            var scoreResult = new { Status = status, Points = card.GetScore(ScoreCategory.Choice), card.Total };
            Assert.Equal(scoreExpected, scoreResult);
        }

        [Fact]
        public void Score_CategoryAlreadyFilled_ReturnsCategoryFilledAndKeepsThePoints()
        {
            ScoreCard card = CreateCard();
            card.Score(ScoreCategory.Choice, NoCombination);
            var scoreExpected = new { Status = GameActionStatus.CategoryFilled, Points = NoCombinationChoicePoints };

            GameActionStatus status = card.Score(ScoreCategory.Choice, YachtOfSixes);

            var scoreResult = new { Status = status, Points = card.GetScore(ScoreCategory.Choice) };
            Assert.Equal(scoreExpected, scoreResult);
        }

        [Fact]
        public void GetScore_OpenCategory_ReturnsZeroAndKeepsItOpen()
        {
            ScoreCard card = CreateCard();
            var categoryExpected = new { Points = NoPoints, IsFilled = false };

            var categoryResult = new
            {
                Points = card.GetScore(ScoreCategory.Sixes),
                IsFilled = card.IsFilled(ScoreCategory.Sixes)
            };

            Assert.Equal(categoryExpected, categoryResult);
        }

        [Fact]
        public void Score_ZeroPoints_StillFillsTheCategory()
        {
            ScoreCard card = CreateCard();
            var categoryExpected = new { IsFilled = true, Points = NoPoints };

            card.Score(ScoreCategory.Yacht, NoCombination);

            var categoryResult = new
            {
                IsFilled = card.IsFilled(ScoreCategory.Yacht),
                Points = card.GetScore(ScoreCategory.Yacht)
            };
            Assert.Equal(categoryExpected, categoryResult);
        }

        [Fact]
        public void UpperBonus_UpperSectionReachesSixtyThree_AddsThirtyFivePoints()
        {
            ScoreCard card = CreateCard();
            var cardExpected = new
            {
                UpperSubtotal = UpperBonusThreshold,
                UpperBonus = UpperBonusPoints,
                Total = TotalWithUpperBonus
            };

            ScoreThreeOfEachUpperValue(card);

            Assert.Equivalent(cardExpected, card);
        }

        [Fact]
        public void UpperBonus_UpperSectionBelowSixtyThree_IsZero()
        {
            ScoreCard fewerSixes = CreateCard();
            var cardExpected = new { UpperSubtotal = UpperSubtotalWithTwoSixes, UpperBonus = NoPoints };

            ScoreUpperWithTwoSixes(fewerSixes);

            Assert.Equivalent(cardExpected, fewerSixes);
        }

        [Fact]
        public void YachtBonus_SecondYachtAfterFiftyPoints_AddsOneHundred()
        {
            ScoreCard card = CreateCard();
            card.Score(ScoreCategory.Yacht, YachtOfFours);
            var bonusExpected = new
            {
                YachtBonus = YachtBonusPoints,
                SixesPoints = YachtOfSixesSixesPoints,
                Total = TotalWithYachtBonus
            };

            card.Score(ScoreCategory.Sixes, YachtOfSixes);

            var bonusResult = new { card.YachtBonus, SixesPoints = card.GetScore(ScoreCategory.Sixes), card.Total };
            Assert.Equal(bonusExpected, bonusResult);
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
            var optionsExpected = new { OptionCount = OpenCategoriesAfterOneScore, OffersChoice = false };

            IReadOnlyList<ScoreEntry> options = card.GetOptions(NoCombination);
            Func<ScoreEntry, bool> isSameCategory = option => option.Category == ScoreCategory.Choice;

            var optionsResult = new
            {
                OptionCount = options.Count,
                OffersChoice = options.Any(isSameCategory)
            };
            Assert.Equal(optionsExpected, optionsResult);
        }

        [Fact]
        public void GetOptions_JokerWithMatchingUpperBoxOpen_ReturnsOnlyThatBox()
        {
            ScoreCard card = CreateCard();
            card.Score(ScoreCategory.Yacht, YachtOfSixes);
            ScoreEntry[] optionsExpected = { new ScoreEntry(ScoreCategory.Fours, YachtOfFoursPoints) };

            IReadOnlyList<ScoreEntry> options = card.GetOptions(YachtOfFours);

            Assert.Equivalent(optionsExpected, options);
        }

        [Fact]
        public void GetOptions_JokerWithMatchingUpperBoxFilled_ReturnsOnlyOpenLowerBoxes()
        {
            ScoreCard card = CreateCard();
            card.Score(ScoreCategory.Yacht, YachtOfSixes);
            card.Score(ScoreCategory.Fours, NoCombination);
            var optionsExpected = new
            {
                OffersUpperBox = false,
                OptionCount = OpenLowerBoxesWithJoker,
                BigStraightPoints,
                ChoicePoints = YachtOfFoursPoints
            };

            IReadOnlyList<ScoreEntry> options = card.GetOptions(YachtOfFours);

            var optionsResult = new
            {
                OffersUpperBox =
                    options.Any(option => ScoreCalculator.IsUpper(option.Category)),
                OptionCount = options.Count,
                BigStraightPoints = FindOption(options, ScoreCategory.BigStraight).Points,
                ChoicePoints = FindOption(options, ScoreCategory.Choice).Points
            };
            Assert.Equal(optionsExpected, optionsResult);
        }

        [Fact]
        public void GetOptions_JokerWithLowerSectionFull_ReturnsOpenUpperBoxesWithZeroPoints()
        {
            ScoreCard card = CreateCard();
            FillLowerSection(card);
            card.Score(ScoreCategory.Fours, NoCombination);
            var optionsExpected = new { OptionCount = OpenUpperBoxesWithJoker, IsAllUpper = true, HasOnlyZeros = true };

            IReadOnlyList<ScoreEntry> options = card.GetOptions(YachtOfFours);
            Func<ScoreEntry, bool> hasNoPoints = option => option.Points == NoPoints;

            var optionsResult = new
            {
                OptionCount = options.Count,
                IsAllUpper =
                    options.All(option => ScoreCalculator.IsUpper(option.Category)),
                HasOnlyZeros = options.All(hasNoPoints)
            };
            Assert.Equal(optionsExpected, optionsResult);
        }

        [Fact]
        public void Score_JokerInABoxThatTheRuleDoesNotAllow_ReturnsCategoryNotAllowedAndKeepsTheBoxOpen()
        {
            ScoreCard card = CreateCard();
            card.Score(ScoreCategory.Yacht, YachtOfSixes);
            var scoreExpected = new { Status = GameActionStatus.CategoryNotAllowed, IsFilled = false };

            GameActionStatus status = card.Score(ScoreCategory.Choice, YachtOfFours);

            var scoreResult = new { Status = status, IsFilled = card.IsFilled(ScoreCategory.Choice) };
            Assert.Equal(scoreExpected, scoreResult);
        }

        [Fact]
        public void IsComplete_AllCategoriesFilled_ReturnsTrueWithoutOpenCategories()
        {
            ScoreCard card = CreateCard();
            var completionExpected = new { IsComplete = true, OpenCategoryCount = NoOpenCategories };

            foreach (ScoreCategory category in ScoreCalculator.All)
            {
                card.Score(category, NoCombination);
            }

            var completionResult = new { card.IsComplete, OpenCategoryCount = card.GetOpenCategories().Count };
            Assert.Equal(completionExpected, completionResult);
        }

        [Fact]
        public void IsComplete_SomeCategoryOpen_ReturnsFalse()
        {
            ScoreCard card = CreateCard();

            card.Score(ScoreCategory.Choice, NoCombination);

            Assert.False(card.IsComplete);
        }

        [Fact]
        public void Restore_SavedBoxAndAYachtBonus_AddsThemToTheTotal()
        {
            ScoreCard card = CreateCard();
            var savedBoxes = new[] { new ScoreEntry(ScoreCategory.Yacht, true, YachtPoints) };

            card.Restore(savedBoxes, OneYachtBonus);

            Assert.Equal(YachtPoints + YachtBonusPoints, card.Total);
        }

        [Fact]
        public void Restore_NullBoxes_ThrowsArgumentNullException()
        {
            ScoreCard card = CreateCard();

            Assert.Throws<ArgumentNullException>(() => card.Restore(null, OneYachtBonus));
        }

        private static ScoreCard CreateCard()
        {
            return new ScoreCard(new ScoreCalculator());
        }

        private static ScoreEntry FindOption(IEnumerable<ScoreEntry> options, ScoreCategory category)
        {
            Func<ScoreEntry, bool> isSameCategory = option => option.Category == category;

            return options.Single(isSameCategory);
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
