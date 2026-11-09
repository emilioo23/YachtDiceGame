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
        private const int FiveDice = 5;
        private const int FirstDie = 0;
        private const int FourthDie = 3;

        [Fact]
        public void Parse_NumbersSeparatedBySpaces_ReturnsTheDiceInOrder()
        {
            List<DieDisplay> dice = DiceRowParser.Parse("4 4 4 2 6");

            Assert.Equal(FiveDice, dice.Count);
            Assert.Equal(new[] { 4, 4, 4, 2, 6 }, dice.Select(die => die.Value));
            Assert.DoesNotContain(dice, die => die.IsDimmed);
        }

        [Fact]
        public void Parse_NumberWithMinusSign_MarksTheDieAsDimmed()
        {
            List<DieDisplay> dice = DiceRowParser.Parse("3 3 3 5- 6-");

            Assert.False(dice[FirstDie].IsDimmed);
            Assert.True(dice[FourthDie].IsDimmed);
            Assert.Equal(2, dice.Count(die => die.IsDimmed));
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
