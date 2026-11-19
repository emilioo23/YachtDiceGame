using System;
using Xunit;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Client.Tests.Dice
{
    /// <summary>
    /// The places of the scene where the dice lie.
    /// </summary>
    public class DiceTableLayoutTests
    {
        private const int Precision = 6;
        private const int BelowFirstDie = -1;
        private const int AfterLastDie = 5;
        private const int OneDie = 1;
        private const int TwoDice = 2;
        private const int ThreeDice = 3;
        private const int FirstPosition = 0;
        private const int SecondPosition = 1;
        private const int NoDice = 0;
        private const int MiddleDie = 2;
        private const double CenterOfTheTable = 0;
        private const double HalfSpacing = 0.75;

        [Fact]
        public void GetRowSlot_MiddleDie_IsInTheCenterOfTheTable()
        {
            var slotExpected = new DiceSlot(CenterOfTheTable, CenterOfTheTable);

            DiceSlot slotResult = DiceTableLayout.GetRowSlot(MiddleDie);

            Assert.Equivalent(slotExpected, slotResult);
        }

        [Fact]
        public void GetRowSlot_FirstDie_IsAtTheLeftOfTheCenter()
        {
            double leftExpected = -DiceTableLayout.RowSpacing * MiddleDie;

            double leftResult = DiceTableLayout.GetRowSlot(FirstPosition).X;

            Assert.Equal(leftExpected, leftResult, Precision);
        }

        [Theory]
        [InlineData(BelowFirstDie)]
        [InlineData(AfterLastDie)]
        public void GetRowSlot_PositionOutOfTheFiveDice_ThrowsArgumentOutOfRangeException(int index)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DiceTableLayout.GetRowSlot(index));
        }

        [Fact]
        public void GetRackSlot_OneDie_IsCenteredUnderTheTable()
        {
            var slotExpected = new DiceSlot(CenterOfTheTable, DiceTableLayout.RackDepth);

            DiceSlot slotResult = DiceTableLayout.GetRackSlot(FirstPosition, OneDie);

            Assert.Equivalent(slotExpected, slotResult);
        }

        [Fact]
        public void GetRackSlot_TwoDice_FirstDieIsAtTheLeftOfTheCenter()
        {
            double leftExpected = -HalfSpacing;

            double leftResult = DiceTableLayout.GetRackSlot(FirstPosition, TwoDice).X;

            Assert.Equal(leftExpected, leftResult, Precision);
        }

        [Fact]
        public void GetRackSlot_TwoDice_SecondDieIsAtTheRightOfTheCenter()
        {
            double rightExpected = HalfSpacing;

            double rightResult = DiceTableLayout.GetRackSlot(SecondPosition, TwoDice).X;

            Assert.Equal(rightExpected, rightResult, Precision);
        }

        [Fact]
        public void GetRackSlot_ThreeDice_AllAreAtTheDepthOfTheRow()
        {
            double depthExpected = DiceTableLayout.RackDepth;

            double depthResult = DiceTableLayout.GetRackSlot(SecondPosition, ThreeDice).Z;

            Assert.Equal(depthExpected, depthResult, Precision);
        }

        [Fact]
        public void GetRackSlot_PositionOutsideTheDiceOfTheRow_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DiceTableLayout.GetRackSlot(TwoDice, TwoDice));
        }

        [Fact]
        public void GetRackSlot_NoDice_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DiceTableLayout.GetRackSlot(FirstPosition, NoDice));
        }
    }
}
