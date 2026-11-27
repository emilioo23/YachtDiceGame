using Xunit;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Client.Tests.Dice
{
    /// <summary>
    /// The vectors that the scene of the dice uses.
    /// </summary>
    public class DiceVectorTests
    {
        private const double FirstValue = 1.5;
        private const double SecondValue = -2;
        private const double ThirdValue = 0.25;
        private const double LongDecimalsValue = 1.23456789;
        private const double RoundedValue = 1.235;
        private const int RoundingDecimals = 3;

        [Fact]
        public void Negate_Vector_ChangesTheSignOfEachValue()
        {
            var vectorExpected = new DiceVector(-FirstValue, -SecondValue, -ThirdValue);
            var vector = new DiceVector(FirstValue, SecondValue, ThirdValue);

            DiceVector vectorResult = vector.Negate();

            Assert.Equivalent(vectorExpected, vectorResult);
        }

        [Fact]
        public void Constructor_AnyValues_KeepsTheValuesInOrder()
        {
            var valuesExpected = new { X = FirstValue, Y = SecondValue, Z = ThirdValue };

            var vector = new DiceVector(FirstValue, SecondValue, ThirdValue);

            Assert.Equivalent(valuesExpected, new { vector.X, vector.Y, vector.Z });
        }

        [Fact]
        public void Negate_VectorNegatedTwice_ReturnsTheSameValues()
        {
            var vector = new DiceVector(FirstValue, SecondValue, ThirdValue);

            DiceVector vectorResult = vector.Negate().Negate();

            Assert.Equivalent(vector, vectorResult);
        }

        [Fact]
        public void Negate_Vector_DoesNotChangeTheOriginal()
        {
            var vectorExpected = new DiceVector(FirstValue, SecondValue, ThirdValue);
            var vector = new DiceVector(FirstValue, SecondValue, ThirdValue);

            vector.Negate();

            Assert.Equivalent(vectorExpected, vector);
        }

        [Fact]
        public void Round_NegativeValuesWithLongDecimals_KeepsTheSign()
        {
            var vectorExpected = new DiceVector(-RoundedValue, -RoundedValue, -RoundedValue);
            var vector = new DiceVector(-LongDecimalsValue, -LongDecimalsValue, -LongDecimalsValue);

            DiceVector vectorResult = vector.Round(RoundingDecimals);

            Assert.Equivalent(vectorExpected, vectorResult);
        }

        [Fact]
        public void Round_ValuesWithLongDecimals_KeepsOnlyTheRequestedDecimals()
        {
            var vectorExpected = new DiceVector(RoundedValue, RoundedValue, RoundedValue);
            var vector = new DiceVector(LongDecimalsValue, LongDecimalsValue, LongDecimalsValue);

            DiceVector vectorResult = vector.Round(RoundingDecimals);

            Assert.Equivalent(vectorExpected, vectorResult);
        }
    }
}
