using Xunit;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Client.Tests.Dice
{
    /// <summary>
    /// The movement of the dice in a throw and in a glide.
    /// </summary>
    public class DiceThrowPathTests
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

        [Fact]
        public void Gather_ThrowStart_IsZero()
        {
            double gatherResult = DiceThrowPath.Gather(StartProgress);

            Assert.Equal(NoTravel, gatherResult, Precision);
        }

        [Fact]
        public void Gather_MiddleOfTheFirstPart_IsHalfOfTheWay()
        {
            double gatherResult = DiceThrowPath.Gather(HalfGatherProgress);

            Assert.Equal(HalfTravel, gatherResult, Precision);
        }

        [Fact]
        public void Gather_EndOfTheFirstPart_IsTheWholeWay()
        {
            double gatherResult = DiceThrowPath.Gather(EndOfGatherProgress);

            Assert.Equal(FullTravel, gatherResult, Precision);
        }

        [Fact]
        public void Gather_ThrowEnd_StaysAtTheWholeWay()
        {
            double gatherResult = DiceThrowPath.Gather(EndProgress);

            Assert.Equal(FullTravel, gatherResult, Precision);
        }

        [Fact]
        public void Scatter_WhileTheDieGoesToTheHand_IsZero()
        {
            double scatterResult = DiceThrowPath.Scatter(EndOfGatherProgress);

            Assert.Equal(NoTravel, scatterResult, Precision);
        }

        [Fact]
        public void Scatter_ThrowEnd_IsTheWholeWay()
        {
            double scatterResult = DiceThrowPath.Scatter(EndProgress);

            Assert.Equal(FullTravel, scatterResult, Precision);
        }

        [Fact]
        public void Scatter_HalfOfTheFlight_IsAheadOfTheHalfOfTheWay()
        {
            bool isAheadExpected = true;

            bool isAheadResult = DiceThrowPath.Scatter(ThreeQuartersProgress) > HalfTravel;

            Assert.Equal(isAheadExpected, isAheadResult);
        }

        [Fact]
        public void Height_ThrowStart_IsTheRestHeight()
        {
            double heightResult = DiceThrowPath.Height(StartProgress, RestHeight);

            Assert.Equal(RestHeight, heightResult, Precision);
        }

        [Fact]
        public void Height_FirstBounce_TouchesTheTable()
        {
            double heightResult = DiceThrowPath.Height(FirstBounceProgress, RestHeight);

            Assert.Equal(RestHeight, heightResult, Precision);
        }

        [Fact]
        public void Height_SecondBounce_TouchesTheTable()
        {
            double heightResult = DiceThrowPath.Height(SecondBounceProgress, RestHeight);

            Assert.Equal(RestHeight, heightResult, Precision);
        }

        [Fact]
        public void Height_ThrowEnd_IsTheRestHeight()
        {
            double heightResult = DiceThrowPath.Height(EndProgress, RestHeight);

            Assert.Equal(RestHeight, heightResult, Precision);
        }

        [Fact]
        public void Height_BetweenTwoBounces_IsAboveTheTable()
        {
            bool isAboveExpected = true;

            bool isAboveResult = DiceThrowPath.Height(BetweenBouncesProgress, RestHeight) > RestHeight;

            Assert.Equal(isAboveExpected, isAboveResult);
        }

        [Fact]
        public void Height_ProgressBeforeTheStart_IsTheHeightOfTheStart()
        {
            double heightExpected = DiceThrowPath.Height(StartProgress, RestHeight);

            double heightResult = DiceThrowPath.Height(BeforeStartProgress, RestHeight);

            Assert.Equal(heightExpected, heightResult, Precision);
        }

        [Fact]
        public void Height_ProgressAfterTheEnd_IsTheHeightOfTheEnd()
        {
            double heightExpected = DiceThrowPath.Height(EndProgress, RestHeight);

            double heightResult = DiceThrowPath.Height(AfterEndProgress, RestHeight);

            Assert.Equal(heightExpected, heightResult, Precision);
        }

        [Fact]
        public void TumbleDegrees_WhileTheDieGoesToTheHand_IsTheTotal()
        {
            double degreesResult = DiceThrowPath.TumbleDegrees(TotalDegrees, EndOfGatherProgress);

            Assert.Equal(TotalDegrees, degreesResult, Precision);
        }

        [Fact]
        public void TumbleDegrees_ThrowEnd_IsZero()
        {
            double degreesResult = DiceThrowPath.TumbleDegrees(TotalDegrees, EndProgress);

            Assert.Equal(NoDegrees, degreesResult, Precision);
        }

        [Fact]
        public void GlideTravel_GlideStart_IsZero()
        {
            double travelResult = DiceThrowPath.GlideTravel(StartProgress);

            Assert.Equal(NoTravel, travelResult, Precision);
        }

        [Fact]
        public void GlideTravel_MiddleOfTheGlide_IsHalfOfTheWay()
        {
            double travelResult = DiceThrowPath.GlideTravel(HalfProgress);

            Assert.Equal(HalfTravel, travelResult, Precision);
        }

        [Fact]
        public void GlideTravel_GlideEnd_IsTheWholeWay()
        {
            double travelResult = DiceThrowPath.GlideTravel(EndProgress);

            Assert.Equal(FullTravel, travelResult, Precision);
        }

        [Fact]
        public void GlideHeight_GlideEnd_IsTheRestHeight()
        {
            double heightResult = DiceThrowPath.GlideHeight(EndProgress, RestHeight);

            Assert.Equal(RestHeight, heightResult, Precision);
        }

        [Fact]
        public void GlideHeight_MiddleOfTheGlide_IsAboveTheTable()
        {
            bool isAboveExpected = true;

            bool isAboveResult = DiceThrowPath.GlideHeight(HalfProgress, RestHeight) > RestHeight;

            Assert.Equal(isAboveExpected, isAboveResult);
        }
    }
}
