using Xunit;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Client.Tests.Dice
{
    /// <summary>
    /// The movement of a die that is thrown on the table.
    /// </summary>
    public class DiceThrowPathTests
    {
        private const int Precision = 6;
        private const double RestHeight = 0.85;
        private const double TotalDegrees = 720;
        private const double StartProgress = 0;
        private const double FirstBounceProgress = 0.2;
        private const double BetweenBouncesProgress = 0.4;
        private const double SecondBounceProgress = 0.6;
        private const double HalfProgress = 0.5;
        private const double EndProgress = 1;
        private const double BeforeStartProgress = -3;
        private const double AfterEndProgress = 4;
        private const double NoDegrees = 0;
        private const double NoTravel = 0;
        private const double FullTravel = 1;

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
        public void Travel_ThrowStart_IsZero()
        {
            double travelResult = DiceThrowPath.Travel(StartProgress);

            Assert.Equal(NoTravel, travelResult, Precision);
        }

        [Fact]
        public void Travel_ThrowEnd_IsTheWholeWay()
        {
            double travelResult = DiceThrowPath.Travel(EndProgress);

            Assert.Equal(FullTravel, travelResult, Precision);
        }

        [Fact]
        public void Travel_HalfOfTheThrow_IsAheadOfTheHalfOfTheWay()
        {
            bool isAheadExpected = true;

            bool isAheadResult = DiceThrowPath.Travel(HalfProgress) > HalfProgress;

            Assert.Equal(isAheadExpected, isAheadResult);
        }

        [Fact]
        public void TumbleDegrees_ThrowStart_IsTheTotal()
        {
            double degreesResult = DiceThrowPath.TumbleDegrees(TotalDegrees, StartProgress);

            Assert.Equal(TotalDegrees, degreesResult, Precision);
        }

        [Fact]
        public void TumbleDegrees_ThrowEnd_IsZero()
        {
            double degreesResult = DiceThrowPath.TumbleDegrees(TotalDegrees, EndProgress);

            Assert.Equal(NoDegrees, degreesResult, Precision);
        }
    }
}
