using System;
using System.Collections.Generic;
using Xunit;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Client.Tests.Dice
{
    /// <summary>
    /// Path of the throw of a die and the place where it lands.
    /// </summary>
    public class DiceMotionTests
    {
        private const int Precision = 6;
        private const double RestHeight = 0.55;
        private const double TotalDegrees = 720;
        private const double StartProgress = 0;
        private const double HalfGatherProgress = 0.1;
        private const double EndOfGatherProgress = 0.2;
        private const double FirstBounceProgress = 0.36;
        private const double BetweenBouncesProgress = 0.52;
        private const double SecondBounceProgress = 0.68;
        private const double HalfProgress = 0.5;
        private const double ThreeQuartersProgress = 0.6;
        private const double EndProgress = 1;
        private const double BeforeStartProgress = -3;
        private const double AfterEndProgress = 4;
        private const double NoDegrees = 0;
        private const double NoTravel = 0;
        private const double HalfTravel = 0.5;
        private const double FullTravel = 1;
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
        public void Gather_ThrowStart_IsZero()
        {
            double gatherResult = DiceMotion.Gather(StartProgress);

            Assert.Equal(NoTravel, gatherResult, Precision);
        }

        [Fact]
        public void Gather_MiddleOfTheFirstPart_IsHalfOfTheWay()
        {
            double gatherResult = DiceMotion.Gather(HalfGatherProgress);

            Assert.Equal(HalfTravel, gatherResult, Precision);
        }

        [Fact]
        public void Gather_EndOfTheFirstPart_IsTheWholeWay()
        {
            double gatherResult = DiceMotion.Gather(EndOfGatherProgress);

            Assert.Equal(FullTravel, gatherResult, Precision);
        }

        [Fact]
        public void Gather_ThrowEnd_StaysAtTheWholeWay()
        {
            double gatherResult = DiceMotion.Gather(EndProgress);

            Assert.Equal(FullTravel, gatherResult, Precision);
        }

        [Fact]
        public void Scatter_WhileTheDieGoesToTheHand_IsZero()
        {
            double scatterResult = DiceMotion.Scatter(EndOfGatherProgress);

            Assert.Equal(NoTravel, scatterResult, Precision);
        }

        [Fact]
        public void Scatter_ThrowEnd_IsTheWholeWay()
        {
            double scatterResult = DiceMotion.Scatter(EndProgress);

            Assert.Equal(FullTravel, scatterResult, Precision);
        }

        [Fact]
        public void Scatter_HalfOfTheFlight_IsAheadOfTheHalfOfTheWay()
        {
            bool isAheadExpected = true;

            bool isAheadResult = DiceMotion.Scatter(ThreeQuartersProgress) > HalfTravel;

            Assert.Equal(isAheadExpected, isAheadResult);
        }

        [Fact]
        public void Height_ThrowStart_IsTheRestHeight()
        {
            double heightResult = DiceMotion.Height(StartProgress, RestHeight);

            Assert.Equal(RestHeight, heightResult, Precision);
        }

        [Fact]
        public void Height_FirstBounce_TouchesTheTable()
        {
            double heightResult = DiceMotion.Height(FirstBounceProgress, RestHeight);

            Assert.Equal(RestHeight, heightResult, Precision);
        }

        [Fact]
        public void Height_SecondBounce_TouchesTheTable()
        {
            double heightResult = DiceMotion.Height(SecondBounceProgress, RestHeight);

            Assert.Equal(RestHeight, heightResult, Precision);
        }

        [Fact]
        public void Height_ThrowEnd_IsTheRestHeight()
        {
            double heightResult = DiceMotion.Height(EndProgress, RestHeight);

            Assert.Equal(RestHeight, heightResult, Precision);
        }

        [Fact]
        public void Height_BetweenTwoBounces_IsAboveTheTable()
        {
            bool isAboveExpected = true;

            bool isAboveResult = DiceMotion.Height(BetweenBouncesProgress, RestHeight) > RestHeight;

            Assert.Equal(isAboveExpected, isAboveResult);
        }

        [Fact]
        public void Height_ProgressBeforeTheStart_IsTheHeightOfTheStart()
        {
            double heightExpected = DiceMotion.Height(StartProgress, RestHeight);

            double heightResult = DiceMotion.Height(BeforeStartProgress, RestHeight);

            Assert.Equal(heightExpected, heightResult, Precision);
        }

        [Fact]
        public void Height_ProgressAfterTheEnd_IsTheHeightOfTheEnd()
        {
            double heightExpected = DiceMotion.Height(EndProgress, RestHeight);

            double heightResult = DiceMotion.Height(AfterEndProgress, RestHeight);

            Assert.Equal(heightExpected, heightResult, Precision);
        }

        [Fact]
        public void TumbleDegrees_WhileTheDieGoesToTheHand_IsTheTotal()
        {
            double degreesResult = DiceMotion.TumbleDegrees(TotalDegrees, EndOfGatherProgress);

            Assert.Equal(TotalDegrees, degreesResult, Precision);
        }

        [Fact]
        public void TumbleDegrees_ThrowEnd_IsZero()
        {
            double degreesResult = DiceMotion.TumbleDegrees(TotalDegrees, EndProgress);

            Assert.Equal(NoDegrees, degreesResult, Precision);
        }

        [Fact]
        public void GlideTravel_GlideStart_IsZero()
        {
            double travelResult = DiceMotion.GlideTravel(StartProgress);

            Assert.Equal(NoTravel, travelResult, Precision);
        }

        [Fact]
        public void GlideTravel_MiddleOfTheGlide_IsHalfOfTheWay()
        {
            double travelResult = DiceMotion.GlideTravel(HalfProgress);

            Assert.Equal(HalfTravel, travelResult, Precision);
        }

        [Fact]
        public void GlideTravel_GlideEnd_IsTheWholeWay()
        {
            double travelResult = DiceMotion.GlideTravel(EndProgress);

            Assert.Equal(FullTravel, travelResult, Precision);
        }

        [Fact]
        public void GlideHeight_GlideEnd_IsTheRestHeight()
        {
            double heightResult = DiceMotion.GlideHeight(EndProgress, RestHeight);

            Assert.Equal(RestHeight, heightResult, Precision);
        }

        [Fact]
        public void GlideHeight_MiddleOfTheGlide_IsAboveTheTable()
        {
            bool isAboveExpected = true;

            bool isAboveResult = DiceMotion.GlideHeight(HalfProgress, RestHeight) > RestHeight;

            Assert.Equal(isAboveExpected, isAboveResult);
        }

        [Fact]
        public void Choose_NoDiceOnTheTable_StaysInsideTheLandingRadius()
        {
            bool isInsideExpected = true;
            var occupied = new List<DiceSlot>();

            DiceSlot slot = DiceMotion.Choose(occupied, Cycle(FarUnit, SideUnit));

            double distance = Math.Sqrt((slot.X * slot.X) + (slot.Z * slot.Z));
            bool isInsideResult = distance <= DiceTableLayout.LandingRadius + Tolerance;
            Assert.Equal(isInsideExpected, isInsideResult);
        }

        [Fact]
        public void Choose_DieOnTheFirstCandidate_ChoosesAPlaceFarEnoughFromIt()
        {
            bool isApartExpected = true;
            var occupied = new List<DiceSlot> { new DiceSlot(CandidateX, CandidateZ) };

            DiceSlot slot = DiceMotion.Choose(
                occupied,
                Cycle(RadiusUnit, AngleUnit, FarRadiusUnit, OppositeAngleUnit));

            double deltaX = slot.X - CandidateX;
            double deltaZ = slot.Z - CandidateZ;
            double distance = Math.Sqrt((deltaX * deltaX) + (deltaZ * deltaZ));
            bool isApartResult = distance >= DiceMotion.MinimumGap;
            Assert.Equal(isApartExpected, isApartResult);
        }

        [Fact]
        public void Choose_NoPlaceIsFree_ReturnsTheFarthestCandidate()
        {
            double candidateX = DiceTableLayout.LandingRadius * Math.Sqrt(RadiusUnit);
            var slotExpected = new DiceSlot(candidateX, CandidateZ);
            var occupied = new List<DiceSlot> { new DiceSlot(CandidateX, CandidateZ) };

            DiceSlot slotResult = DiceMotion.Choose(occupied, Cycle(RadiusUnit, AngleUnit));

            Assert.Equivalent(slotExpected, slotResult);
        }

        private static Func<double> Cycle(params double[] values)
        {
            var position = 0;

            return () => values[position++ % values.Length];
        }
    }
}
