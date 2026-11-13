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
        private const int FaceOne = 1;
        private const int FaceTwo = 2;
        private const int FaceThree = 3;
        private const int FaceFour = 4;
        private const int FaceFive = 5;
        private const int FaceSix = 6;

        [Fact]
        public void Parse_NumbersSeparatedBySpaces_ReturnsTheDiceInOrder()
        {
            DieDisplay[] expected =
            {
                new DieDisplay { Value = FaceFour, IsDimmed = false },
                new DieDisplay { Value = FaceFour, IsDimmed = false },
                new DieDisplay { Value = FaceFour, IsDimmed = false },
                new DieDisplay { Value = FaceTwo, IsDimmed = false },
                new DieDisplay { Value = FaceSix, IsDimmed = false }
            };

            List<DieDisplay> dice = DiceRowParser.Parse(ThreeFoursRow);

            Assert.Equivalent(expected, dice, strict: true);
        }

        [Fact]
        public void Parse_NumberWithMinusSign_MarksTheDieAsDimmed()
        {
            DieDisplay[] expected =
            {
                new DieDisplay { Value = FaceThree, IsDimmed = false },
                new DieDisplay { Value = FaceThree, IsDimmed = false },
                new DieDisplay { Value = FaceThree, IsDimmed = false },
                new DieDisplay { Value = FaceFive, IsDimmed = true },
                new DieDisplay { Value = FaceSix, IsDimmed = true }
            };

            List<DieDisplay> dice = DiceRowParser.Parse(LastTwoDimmedRow);

            Assert.Equivalent(expected, dice, strict: true);
        }

        [Fact]
        public void Parse_LowestAndHighestFace_AreAccepted()
        {
            DieDisplay[] expected =
            {
                new DieDisplay { Value = FaceOne, IsDimmed = false },
                new DieDisplay { Value = FaceSix, IsDimmed = false }
            };

            List<DieDisplay> dice = DiceRowParser.Parse(LowestAndHighestRow);

            Assert.Equivalent(expected, dice, strict: true);
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
                new DieDisplay { Value = FaceTwo, IsDimmed = false },
                new DieDisplay { Value = FaceFive, IsDimmed = false }
            };

            List<DieDisplay> dice = DiceRowParser.Parse(InvalidWordBetweenRow);

            Assert.Equivalent(expected, dice, strict: true);
        }

        [Fact]
        public void Parse_ExtraSpaces_AreIgnored()
        {
            DieDisplay[] expected =
            {
                new DieDisplay { Value = FaceOne, IsDimmed = false },
                new DieDisplay { Value = FaceTwo, IsDimmed = false }
            };

            List<DieDisplay> dice = DiceRowParser.Parse(ExtraSpacesRow);

            Assert.Equivalent(expected, dice, strict: true);
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
