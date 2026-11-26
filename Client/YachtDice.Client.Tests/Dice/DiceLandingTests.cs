using System;
using System.Collections.Generic;
using Xunit;
using YachtDice.Client.Logic.Dice;
using YachtDice.Client.Tests.InMemory;

namespace YachtDice.Client.Tests.Dice
{
    /// <summary>
    /// The place where a die lands on the table.
    /// </summary>
    public class DiceLandingTests
    {
        private const double RadiusUnit = 0.25;
        private const double AngleUnit = 0;
        private const double FarRadiusUnit = 1;
        private const double OppositeAngleUnit = 0.5;
        private const double FarUnit = 0.9;
        private const double SideUnit = 0.3;
        private const double CandidateX = 1.3;
        private const double CandidateZ = 0;
        private const double Tolerance = 0.000001;

        [Fact]
        public void Choose_NoDiceOnTheTable_StaysInsideTheLandingRadius()
        {
            bool isInsideExpected = true;
            var occupied = new List<DiceSlot>();

            DiceSlot slot = DiceLanding.Choose(occupied, new InMemoryUnitSequence(FarUnit, SideUnit).Next);

            double distance = Math.Sqrt((slot.X * slot.X) + (slot.Z * slot.Z));
            bool isInsideResult = distance <= DiceTableLayout.LandingRadius + Tolerance;
            Assert.Equal(isInsideExpected, isInsideResult);
        }

        [Fact]
        public void Choose_DieOnTheFirstCandidate_ChoosesAPlaceFarEnoughFromIt()
        {
            bool isApartExpected = true;
            var occupied = new List<DiceSlot> { new DiceSlot(CandidateX, CandidateZ) };

            DiceSlot slot = DiceLanding.Choose(
                occupied,
                new InMemoryUnitSequence(RadiusUnit, AngleUnit, FarRadiusUnit, OppositeAngleUnit).Next);

            double deltaX = slot.X - CandidateX;
            double deltaZ = slot.Z - CandidateZ;
            double distance = Math.Sqrt((deltaX * deltaX) + (deltaZ * deltaZ));
            bool isApartResult = distance >= DiceLanding.MinimumGap;
            Assert.Equal(isApartExpected, isApartResult);
        }

        [Fact]
        public void Choose_NoPlaceIsFree_ReturnsTheFarthestCandidate()
        {
            double candidateX = DiceTableLayout.LandingRadius * Math.Sqrt(RadiusUnit);
            var slotExpected = new DiceSlot(candidateX, CandidateZ);
            var occupied = new List<DiceSlot> { new DiceSlot(CandidateX, CandidateZ) };

            DiceSlot slotResult = DiceLanding.Choose(occupied, new InMemoryUnitSequence(RadiusUnit, AngleUnit).Next);

            Assert.Equivalent(slotExpected, slotResult);
        }
    }
}
