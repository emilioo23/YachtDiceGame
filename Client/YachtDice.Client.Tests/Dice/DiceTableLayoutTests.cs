using System;
using Xunit;
using YachtDice.Client.Logic.Dice;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Dice
{
    /// <summary>
    /// The places of the table where the five dice lie.
    /// </summary>
    public class DiceTableLayoutTests
    {
        private const int Precision = 6;
        private const double CubeDiagonal = 2.4;
        private const int BelowFirstDie = -1;
        private const int AfterLastDie = 5;

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void GetSlot_EachDie_IsAtTheSlotRadiusFromTheCenter(int index)
        {
            DiceSlot slot = DiceTableLayout.GetSlot(index);

            double distanceResult = Math.Sqrt((slot.X * slot.X) + (slot.Z * slot.Z));

            Assert.Equal(DiceTableLayout.SlotRadius, distanceResult, Precision);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void GetSlot_NextDie_IsFartherThanTheDiagonalOfACube(int index)
        {
            bool isApartExpected = true;
            DiceSlot slot = DiceTableLayout.GetSlot(index);
            DiceSlot next = DiceTableLayout.GetSlot((index + 1) % HeldDiceSelection.DiceCount);

            double deltaX = slot.X - next.X;
            double deltaZ = slot.Z - next.Z;
            double distance = Math.Sqrt((deltaX * deltaX) + (deltaZ * deltaZ));
            bool isApartResult = distance > CubeDiagonal;

            Assert.Equal(isApartExpected, isApartResult);
        }

        [Theory]
        [InlineData(BelowFirstDie)]
        [InlineData(AfterLastDie)]
        public void GetSlot_PositionOutOfTheFiveDice_ThrowsArgumentOutOfRangeException(int index)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DiceTableLayout.GetSlot(index));
        }
    }
}
