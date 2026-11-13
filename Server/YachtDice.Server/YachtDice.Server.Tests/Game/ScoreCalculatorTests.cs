using System;
using Xunit;
using YachtDice.Server.Logic.Game;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// Points of each box of the scorecard for a roll.
    /// </summary>
    public class ScoreCalculatorTests
    {
        private const int NoCombinationChoicePoints = 16;
        private const int UnknownCategoryValue = 99;

        private static readonly int[] ThreeFoursAndTwoOthers = { 4, 4, 4, 1, 2 };
        private static readonly int[] NoCombination = { 1, 2, 3, 4, 6 };
        private static readonly int[] YachtOfThrees = { 3, 3, 3, 3, 3 };
        private static readonly int[] OnlyFourDice = { 1, 2, 3, 4 };
        private static readonly int[] DieWithSeven = { 1, 2, 3, 4, 7 };
        private static readonly int[] LittleStraight = { 1, 2, 3, 4, 5 };

        [Theory]
        [InlineData(ScoreCategory.Ones, new[] { 1, 1, 1, 4, 5 }, 3)]
        [InlineData(ScoreCategory.Twos, new[] { 2, 2, 3, 4, 5 }, 4)]
        [InlineData(ScoreCategory.Threes, new[] { 3, 3, 3, 3, 5 }, 12)]
        [InlineData(ScoreCategory.Fours, new[] { 1, 2, 3, 5, 6 }, 0)]
        [InlineData(ScoreCategory.Fives, new[] { 5, 5, 5, 5, 5 }, 25)]
        [InlineData(ScoreCategory.Sixes, new[] { 6, 6, 1, 2, 3 }, 12)]
        public void Calculate_UpperCategory_SumsOnlyTheDiceOfThatValue(
            ScoreCategory category,
            int[] dice,
            int expectedPoints)
        {
            var calculator = new ScoreCalculator();

            var points = calculator.Calculate(category, dice);

            Assert.Equal(expectedPoints, points);
        }

        [Theory]
        [InlineData(new[] { 3, 3, 3, 5, 5 }, 19)]
        [InlineData(new[] { 6, 1, 6, 1, 6 }, 20)]
        public void Calculate_FullHouseWithTripleAndPair_ReturnsSumOfFiveDice(int[] dice, int expectedPoints)
        {
            var calculator = new ScoreCalculator();

            var points = calculator.Calculate(ScoreCategory.FullHouse, dice);

            Assert.Equal(expectedPoints, points);
        }

        [Theory]
        [InlineData(new[] { 4, 4, 4, 4, 1 })]
        [InlineData(new[] { 2, 2, 3, 3, 5 })]
        [InlineData(new[] { 6, 6, 6, 6, 6 })]
        public void Calculate_FullHouseWithoutTripleAndPair_ReturnsZero(int[] dice)
        {
            var calculator = new ScoreCalculator();

            var points = calculator.Calculate(ScoreCategory.FullHouse, dice);

            Assert.Equal(0, points);
        }

        [Theory]
        [InlineData(new[] { 5, 5, 5, 5, 2 }, 20)]
        [InlineData(new[] { 6, 6, 6, 6, 6 }, 24)]
        [InlineData(new[] { 1, 3, 3, 3, 3 }, 12)]
        public void Calculate_FourOfAKind_SumsOnlyFourDice(int[] dice, int expectedPoints)
        {
            var calculator = new ScoreCalculator();

            var points = calculator.Calculate(ScoreCategory.FourOfAKind, dice);

            Assert.Equal(expectedPoints, points);
        }

        [Fact]
        public void Calculate_FourOfAKindWithOnlyThreeEqualDice_ReturnsZero()
        {
            var calculator = new ScoreCalculator();

            var points = calculator.Calculate(ScoreCategory.FourOfAKind, ThreeFoursAndTwoOthers);

            Assert.Equal(0, points);
        }

        [Theory]
        [InlineData(new[] { 1, 2, 3, 4, 5 }, 30)]
        [InlineData(new[] { 5, 4, 3, 2, 1 }, 30)]
        [InlineData(new[] { 2, 3, 4, 5, 6 }, 0)]
        [InlineData(new[] { 1, 2, 3, 4, 4 }, 0)]
        public void Calculate_LittleStraight_PaysOnlyForOneToFive(int[] dice, int expectedPoints)
        {
            var calculator = new ScoreCalculator();

            var points = calculator.Calculate(ScoreCategory.LittleStraight, dice);

            Assert.Equal(expectedPoints, points);
        }

        [Theory]
        [InlineData(new[] { 2, 3, 4, 5, 6 }, 30)]
        [InlineData(new[] { 6, 5, 4, 3, 2 }, 30)]
        [InlineData(new[] { 1, 2, 3, 4, 5 }, 0)]
        [InlineData(new[] { 2, 3, 4, 6, 6 }, 0)]
        public void Calculate_BigStraight_PaysOnlyForTwoToSix(int[] dice, int expectedPoints)
        {
            var calculator = new ScoreCalculator();

            var points = calculator.Calculate(ScoreCategory.BigStraight, dice);

            Assert.Equal(expectedPoints, points);
        }

        [Fact]
        public void Calculate_Choice_ReturnsSumOfAllDice()
        {
            var calculator = new ScoreCalculator();

            var points = calculator.Calculate(ScoreCategory.Choice, NoCombination);

            Assert.Equal(NoCombinationChoicePoints, points);
        }

        [Theory]
        [InlineData(new[] { 6, 6, 6, 6, 6 }, 50)]
        [InlineData(new[] { 1, 1, 1, 1, 1 }, 50)]
        [InlineData(new[] { 6, 6, 6, 6, 5 }, 0)]
        public void Calculate_Yacht_PaysFiftyOnlyWhenAllDiceAreEqual(int[] dice, int expectedPoints)
        {
            var calculator = new ScoreCalculator();

            var points = calculator.Calculate(ScoreCategory.Yacht, dice);

            Assert.Equal(expectedPoints, points);
        }

        [Theory]
        [InlineData(ScoreCategory.FullHouse, 15)]
        [InlineData(ScoreCategory.LittleStraight, 30)]
        [InlineData(ScoreCategory.BigStraight, 30)]
        [InlineData(ScoreCategory.FourOfAKind, 12)]
        [InlineData(ScoreCategory.Choice, 15)]
        [InlineData(ScoreCategory.Threes, 15)]
        [InlineData(ScoreCategory.Sixes, 0)]
        public void CalculateJoker_YachtOfThrees_UsesTheJokerValueOfEachBox(ScoreCategory category, int expectedPoints)
        {
            var calculator = new ScoreCalculator();
            var yachtOfThrees = YachtOfThrees;

            var points = calculator.CalculateJoker(category, yachtOfThrees);

            Assert.Equal(expectedPoints, points);
        }

        [Theory]
        [InlineData(new[] { 2, 2, 2, 2, 2 }, true)]
        [InlineData(new[] { 2, 2, 2, 2, 3 }, false)]
        public void IsYacht_Dice_TellsWhetherAllAreEqual(int[] dice, bool isExpectedYacht)
        {
            var calculator = new ScoreCalculator();

            var isYacht = calculator.IsYacht(dice);

            Assert.Equal(isExpectedYacht, isYacht);
        }

        [Fact]
        public void Calculate_FourDice_ThrowsArgumentException()
        {
            var calculator = new ScoreCalculator();

            Assert.Throws<ArgumentException>(() => calculator.Calculate(ScoreCategory.Choice, OnlyFourDice));
        }

        [Fact]
        public void Calculate_DieWithValueSeven_ThrowsArgumentException()
        {
            var calculator = new ScoreCalculator();

            Assert.Throws<ArgumentException>(() => calculator.Calculate(ScoreCategory.Choice, DieWithSeven));
        }

        [Fact]
        public void Calculate_UnknownCategory_ThrowsArgumentOutOfRangeException()
        {
            var calculator = new ScoreCalculator();
            var unknownCategory = (ScoreCategory)UnknownCategoryValue;

            Assert.Throws<ArgumentOutOfRangeException>(
                () => calculator.Calculate(unknownCategory, LittleStraight));
        }
    }
}
