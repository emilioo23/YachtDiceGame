using System;
using System.Collections.Generic;
using Xunit;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Client.Tests.Dice
{
    /// <summary>
    /// Faces of the dice.
    /// </summary>
    public class DiceFacesTests
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
        private const int FourthFace = 4;
        private const string FourthFaceKey = "Die_4";
        private const int SixthFace = 6;
        private const string SixthFaceKey = "Die_6";
        private const int SeventhFace = 7;
        private const string SeventhFaceKey = "Die_7";
        private const int NoFace = 0;
        private const string NoFaceKey = "Die_0";
        private const int NegativeFace = -1;
        private const string NegativeFaceKey = "Die_-1";
        private const string FirstFaceKey = "Die_1";
        private const int FaceSeven = 7;
        private const int Low = 0;
        private const int Middle = 1;
        private const int High = 2;

        [Fact]
        public void Parse_NumbersSeparatedBySpaces_ReturnsTheDiceInOrder()
        {
            DieDisplay[] diceExpected =
            {
                new DieDisplay { Value = FaceFour, IsDimmed = false },
                new DieDisplay { Value = FaceFour, IsDimmed = false },
                new DieDisplay { Value = FaceFour, IsDimmed = false },
                new DieDisplay { Value = FaceTwo, IsDimmed = false },
                new DieDisplay { Value = FaceSix, IsDimmed = false }
            };

            List<DieDisplay> dice = DiceFaces.Parse(ThreeFoursRow);

            Assert.Equivalent(diceExpected, dice, strict: true);
        }

        [Fact]
        public void Parse_NumberWithMinusSign_MarksTheDieAsDimmed()
        {
            DieDisplay[] diceExpected =
            {
                new DieDisplay { Value = FaceThree, IsDimmed = false },
                new DieDisplay { Value = FaceThree, IsDimmed = false },
                new DieDisplay { Value = FaceThree, IsDimmed = false },
                new DieDisplay { Value = FaceFive, IsDimmed = true },
                new DieDisplay { Value = FaceSix, IsDimmed = true }
            };

            List<DieDisplay> dice = DiceFaces.Parse(LastTwoDimmedRow);

            Assert.Equivalent(diceExpected, dice, strict: true);
        }

        [Fact]
        public void Parse_LowestAndHighestFace_AreAccepted()
        {
            DieDisplay[] diceExpected =
            {
                new DieDisplay { Value = FaceOne, IsDimmed = false },
                new DieDisplay { Value = FaceSix, IsDimmed = false }
            };

            List<DieDisplay> dice = DiceFaces.Parse(LowestAndHighestRow);

            Assert.Equivalent(diceExpected, dice, strict: true);
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
            List<DieDisplay> dice = DiceFaces.Parse(text);

            Assert.Empty(dice);
        }

        [Fact]
        public void Parse_InvalidWordBetweenValidOnes_KeepsTheValidOnes()
        {
            DieDisplay[] diceExpected =
            {
                new DieDisplay { Value = FaceTwo, IsDimmed = false },
                new DieDisplay { Value = FaceFive, IsDimmed = false }
            };

            List<DieDisplay> dice = DiceFaces.Parse(InvalidWordBetweenRow);

            Assert.Equivalent(diceExpected, dice, strict: true);
        }

        [Fact]
        public void Parse_ExtraSpaces_AreIgnored()
        {
            DieDisplay[] diceExpected =
            {
                new DieDisplay { Value = FaceOne, IsDimmed = false },
                new DieDisplay { Value = FaceTwo, IsDimmed = false }
            };

            List<DieDisplay> dice = DiceFaces.Parse(ExtraSpacesRow);

            Assert.Equivalent(diceExpected, dice, strict: true);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Parse_EmptyText_ReturnsAnEmptyList(string text)
        {
            List<DieDisplay> dice = DiceFaces.Parse(text);

            Assert.Empty(dice);
        }

        [Fact]
        public void Create_FaceNumber_ReturnsTheNameWithThatNumber()
        {
            Assert.Equal(FourthFaceKey, DiceFaces.Create(FourthFace));
        }

        [Fact]
        public void Create_DefaultValue_ReturnsTheNameOfTheFirstFace()
        {
            string key = DiceFaces.Create(DiceFaces.DefaultValue);

            Assert.Equal(FirstFaceKey, key);
        }

        [Fact]
        public void Create_HighestFace_ReturnsTheNameWithThatNumber()
        {
            string key = DiceFaces.Create(SixthFace);

            Assert.Equal(SixthFaceKey, key);
        }

        [Fact]
        public void Create_Zero_ReturnsTheNameWithZero()
        {
            string key = DiceFaces.Create(NoFace);

            Assert.Equal(NoFaceKey, key);
        }

        [Fact]
        public void Create_NegativeNumber_KeepsTheMinusSign()
        {
            string key = DiceFaces.Create(NegativeFace);

            Assert.Equal(NegativeFaceKey, key);
        }

        [Fact]
        public void Create_NumberAboveTheFaces_ReturnsTheNameWithThatNumber()
        {
            string key = DiceFaces.Create(SeventhFace);

            Assert.Equal(SeventhFaceKey, key);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public void GetCells_EachValue_HasAsManyPipsAsItsValue(int value)
        {
            int pipCountExpected = value;

            int pipCountResult = DiceFaces.GetCells(value).Count;

            Assert.Equal(pipCountExpected, pipCountResult);
        }

        [Fact]
        public void GetCells_FaceFive_HasTheFourCornersAndTheCenter()
        {
            var cellsExpected = new List<PipCell>
            {
                new PipCell(Low, Low),
                new PipCell(Low, High),
                new PipCell(Middle, Middle),
                new PipCell(High, Low),
                new PipCell(High, High)
            };

            IReadOnlyList<PipCell> cellsResult = DiceFaces.GetCells(FaceFive);

            Assert.Equivalent(cellsExpected, cellsResult);
        }

        [Fact]
        public void GetCells_ValueThatIsNotAFace_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DiceFaces.GetCells(FaceSeven));
        }
    }
}
