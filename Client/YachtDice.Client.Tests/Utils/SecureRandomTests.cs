using System;
using System.Collections.Generic;
using Xunit;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Secure random numbers used by the confetti of the Yacht celebration.
    /// </summary>
    public class SecureRandomTests
    {
        private const int Repetitions = 300;
        private const int Maximum = 3;
        private const int FirstValue = 0;
        private const int LastValue = Maximum - 1;
        private const int SingleValueMaximum = 1;
        private const int InvalidMaximum = 0;
        private const int SingleDistinctValue = 1;
        private const double UnitMinimum = 0;
        private const double UnitMaximum = 1;

        [Fact]
        public void Next_MaximumOfThree_StaysInsideTheRange()
        {
            var random = new SecureRandom();

            int value = random.Next(Maximum);

            Assert.InRange(value, FirstValue, LastValue);
        }

        [Fact]
        public void Next_ManyDraws_ProducesEveryValueOfTheRange()
        {
            var random = new SecureRandom();
            var values = new HashSet<int>();

            for (int repetition = 0; repetition < Repetitions; repetition++)
            {
                values.Add(random.Next(Maximum));
            }

            Assert.Equal(Maximum, values.Count);
        }

        [Fact]
        public void Next_MaximumOfOne_ReturnsZero()
        {
            var random = new SecureRandom();

            int value = random.Next(SingleValueMaximum);

            Assert.Equal(FirstValue, value);
        }

        [Fact]
        public void Next_MaximumNotGreaterThanZero_ThrowsArgumentOutOfRangeException()
        {
            var random = new SecureRandom();
            Action next = () => random.Next(InvalidMaximum);

            Assert.Throws<ArgumentOutOfRangeException>(next);
        }

        [Fact]
        public void NextDouble_AnyDraw_StaysInsideTheUnitRange()
        {
            var random = new SecureRandom();

            double value = random.NextDouble();

            Assert.InRange(value, UnitMinimum, UnitMaximum);
        }

        [Fact]
        public void NextDouble_ManyDraws_ProducesDifferentValues()
        {
            var random = new SecureRandom();
            var values = new HashSet<double>();

            for (int repetition = 0; repetition < Repetitions; repetition++)
            {
                values.Add(random.NextDouble());
            }

            Assert.NotEqual(SingleDistinctValue, values.Count);
        }
    }
}
