using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Client.Tests.Dice
{
    /// <summary>
    /// Rules to read the text that describes the dice of an example in the "How to play" window.
    /// </summary>
    public class DiceRowParserTests
    {
        private const string ThreeFoursRow = "4 4 4 2 6";
        private const string LastTwoDimmedRow = "3 3 3 5- 6-";

        private static readonly int[] ThreeFoursValues = { 4, 4, 4, 2, 6 };
        private static readonly bool[] LastTwoDimmed = { false, false, false, true, true };

        [Fact]
        public void Parse_NumbersSeparatedBySpaces_ReturnsTheDiceInOrder()
        {
            var expected = new { Values = ThreeFoursValues, IsAnyDimmed = false };

            List<DieDisplay> dice = DiceRowParser.Parse(ThreeFoursRow);

            var actual = new
            {
                Values =
                    dice.Select(die => die.Value).ToArray(),
                IsAnyDimmed =
                    dice.Any(die => die.IsDimmed)
            };
            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public void Parse_NumberWithMinusSign_MarksTheDieAsDimmed()
        {
            List<DieDisplay> dice = DiceRowParser.Parse(LastTwoDimmedRow);

            Assert.Equal(LastTwoDimmed, dice.Select(die => die.IsDimmed).ToArray());
        }

        [Fact]
        public void Parse_LowestAndHighestFace_AreAccepted()
        {
            List<DieDisplay> dice = DiceRowParser.Parse("1 6");

            Assert.Equal(new[] { 1, 6 }, dice.Select(die => die.Value));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("7")]
        [InlineData("abc")]
        [InlineData("-")]
        [InlineData("3.5")]
        [InlineData("-4")]
        public void Parse_WordThatIsNotAFace_IsIgnored(string text)
        {
            List<DieDisplay> dice = DiceRowParser.Parse(text);

            Assert.Empty(dice);
        }

        [Fact]
        public void Parse_InvalidWordBetweenValidOnes_KeepsTheValidOnes()
        {
            List<DieDisplay> dice = DiceRowParser.Parse("2 x 5");

            Assert.Equal(new[] { 2, 5 }, dice.Select(die => die.Value));
        }

        [Fact]
        public void Parse_ExtraSpaces_AreIgnored()
        {
            List<DieDisplay> dice = DiceRowParser.Parse("  1   2  ");

            Assert.Equal(new[] { 1, 2 }, dice.Select(die => die.Value));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Parse_EmptyText_ReturnsAnEmptyList(string text)
        {
            List<DieDisplay> dice = DiceRowParser.Parse(text);

            Assert.Empty(dice);
        }
    }
}
