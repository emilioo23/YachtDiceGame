using Xunit;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Client.Tests.Dice
{
    /// <summary>
    /// The turn that leaves a face of a die looking up: a vector turned around an axis by some degrees.
    /// </summary>
    public class FaceUpRotationTests
    {
        private const double QuarterTurn = 90;
        private const double HalfTurn = 180;
        private const double FullTurn = 360;
        private const double NoTurn = 0;
        private const int RoundingDecimals = 6;
        private const double One = 1;
        private const double Zero = 0;
        private const double FirstValue = 0.3;
        private const double SecondValue = -0.7;
        private const double ThirdValue = 0.2;

        private static readonly DiceVector AxisX = new DiceVector(One, Zero, Zero);
        private static readonly DiceVector AxisY = new DiceVector(Zero, One, Zero);
        private static readonly DiceVector AxisZ = new DiceVector(Zero, Zero, One);
        private static readonly DiceVector AnyVector = new DiceVector(FirstValue, SecondValue, ThirdValue);

        [Fact]
        public void Constructor_AnyValues_KeepsTheDegrees()
        {
            var rotation = new FaceUpRotation(AxisZ, QuarterTurn);

            Assert.Equal(QuarterTurn, rotation.Degrees);
        }

        [Fact]
        public void Constructor_AnyValues_KeepsTheAxis()
        {
            var rotation = new FaceUpRotation(AxisZ, QuarterTurn);

            Assert.Same(AxisZ, rotation.Axis);
        }

        [Fact]
        public void Rotate_QuarterTurnAroundZ_TurnsTheXAxisIntoTheYAxis()
        {
            var rotation = new FaceUpRotation(AxisZ, QuarterTurn);

            DiceVector vectorResult = rotation.Rotate(AxisX).Round(RoundingDecimals);

            Assert.Equivalent(AxisY, vectorResult);
        }

        [Fact]
        public void Rotate_QuarterTurnAroundX_TurnsTheYAxisIntoTheZAxis()
        {
            var rotation = new FaceUpRotation(AxisX, QuarterTurn);

            DiceVector vectorResult = rotation.Rotate(AxisY).Round(RoundingDecimals);

            Assert.Equivalent(AxisZ, vectorResult);
        }

        [Fact]
        public void Rotate_HalfTurnAroundZ_TurnsTheXAxisToTheOtherSide()
        {
            DiceVector vectorExpected = AxisX.Negate();
            var rotation = new FaceUpRotation(AxisZ, HalfTurn);

            DiceVector vectorResult = rotation.Rotate(AxisX).Round(RoundingDecimals);

            Assert.Equivalent(vectorExpected, vectorResult);
        }

        [Fact]
        public void Rotate_NoTurn_KeepsTheVector()
        {
            DiceVector vectorExpected = AnyVector.Round(RoundingDecimals);
            var rotation = new FaceUpRotation(AxisY, NoTurn);

            DiceVector vectorResult = rotation.Rotate(AnyVector).Round(RoundingDecimals);

            Assert.Equivalent(vectorExpected, vectorResult);
        }

        [Fact]
        public void Rotate_FullTurn_KeepsTheVector()
        {
            DiceVector vectorExpected = AnyVector.Round(RoundingDecimals);
            var rotation = new FaceUpRotation(AxisX, FullTurn);

            DiceVector vectorResult = rotation.Rotate(AnyVector).Round(RoundingDecimals);

            Assert.Equivalent(vectorExpected, vectorResult);
        }

        [Fact]
        public void Rotate_VectorOnTheAxis_KeepsTheVector()
        {
            var rotation = new FaceUpRotation(AxisZ, QuarterTurn);

            DiceVector vectorResult = rotation.Rotate(AxisZ).Round(RoundingDecimals);

            Assert.Equivalent(AxisZ, vectorResult);
        }

        [Fact]
        public void Rotate_TwoQuarterTurns_EndLikeAHalfTurn()
        {
            var quarterTurn = new FaceUpRotation(AxisY, QuarterTurn);
            DiceVector vectorExpected = new FaceUpRotation(AxisY, HalfTurn).Rotate(AnyVector).Round(RoundingDecimals);

            DiceVector vectorResult = quarterTurn.Rotate(quarterTurn.Rotate(AnyVector)).Round(RoundingDecimals);

            Assert.Equivalent(vectorExpected, vectorResult);
        }
    }
}
