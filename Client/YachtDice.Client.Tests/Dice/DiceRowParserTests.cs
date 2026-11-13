using System.Collections.Generic;
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
        private const string LowestAndHighestRow = "1 6";
        private const string InvalidWordBetweenRow = "2 x 5";
        private const string ExtraSpacesRow = "  1   2  ";

        [Fact]
        public void Parse_NumbersSeparatedBySpaces_ReturnsTheDiceInOrder()
        {
            DieDisplay[] expected =
            {
                new DieDisplay { Value = 4, IsDimmed = false },
                new DieDisplay { Value = 4, IsDimmed = false },
                new DieDisplay { Value = 4, IsDimmed = false },
                new DieDisplay { Value = 2, IsDimmed = false },
                new DieDisplay { Value = 6, IsDimmed = false }
            };

            List<DieDisplay> dice = DiceRowParser.Parse(ThreeFoursRow);

            Assert.Equivalent(expected, dice);
        }

        [Fact]
        public void Parse_NumberWithMinusSign_MarksTheDieAsDimmed()
        {
            DieDisplay[] expected =
            {
                new DieDisplay { Value = 3, IsDimmed = false },
                new DieDisplay { Value = 3, IsDimmed = false },
                new DieDisplay { Value = 3, IsDimmed = false },
                new DieDisplay { Value = 5, IsDimmed = true },
                new DieDisplay { Value = 6, IsDimmed = true }
            };

            List<DieDisplay> dice = DiceRowParser.Parse(LastTwoDimmedRow);

            Assert.Equivalent(expected, dice);
        }

        [Fact]
        public void Parse_LowestAndHighestFace_AreAccepted()
        {
            DieDisplay[] expected =
            {
                new DieDisplay { Value = 1, IsDimmed = false },
                new DieDisplay { Value = 6, IsDimmed = false }
            };

            List<DieDisplay> dice = DiceRowParser.Parse(LowestAndHighestRow);

            Assert.Equivalent(expected, dice);
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
            DieDisplay[] expected =
            {
                new DieDisplay { Value = 2, IsDimmed = false },
                new DieDisplay { Value = 5, IsDimmed = false }
            };

            List<DieDisplay> dice = DiceRowParser.Parse(InvalidWordBetweenRow);

            Assert.Equivalent(expected, dice);
        }

        [Fact]
        public void Parse_ExtraSpaces_AreIgnored()
        {
            DieDisplay[] expected =
            {
                new DieDisplay { Value = 1, IsDimmed = false },
                new DieDisplay { Value = 2, IsDimmed = false }
            };

            List<DieDisplay> dice = DiceRowParser.Parse(ExtraSpacesRow);

            Assert.Equivalent(expected, dice);
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
