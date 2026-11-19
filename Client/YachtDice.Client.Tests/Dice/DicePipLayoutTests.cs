using System;
using System.Collections.Generic;
using Xunit;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Client.Tests.Dice
{
    /// <summary>
    /// The cells of the grid that have a pip in each face of a die.
    /// </summary>
    public class DicePipLayoutTests
    {
        private const int FaceFive = 5;
        private const int FaceSeven = 7;
        private const int Low = 0;
        private const int Middle = 1;
        private const int High = 2;

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

            int pipCountResult = DicePipLayout.GetCells(value).Count;

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

            IReadOnlyList<PipCell> cellsResult = DicePipLayout.GetCells(FaceFive);

            Assert.Equivalent(cellsExpected, cellsResult);
        }

        [Fact]
        public void GetCells_ValueThatIsNotAFace_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DicePipLayout.GetCells(FaceSeven));
        }
    }
}
