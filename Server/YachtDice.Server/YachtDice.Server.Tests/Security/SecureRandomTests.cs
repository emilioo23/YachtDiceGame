using System;
using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Tests.Security
{
    /// <summary>
    /// Secure random integers used for the dice and the codes of the rooms.
    /// </summary>
    public class SecureRandomTests
    {
        private const int Repetitions = 600;
        private const int SingleValue = 7;
        private const int NegativeMinimum = -3;
        private const int NegativeMaximum = 3;

        [Fact]
        public void Next_DieRange_StaysInsideTheRange()
        {
            int[] values = Enumerable.Range(0, Repetitions)
                .Select(repetition => RollDie())
                .ToArray();
            Func<int, bool> isNotDieFace = value => !IsDieFace(value);

            int[] outsideValues = values.Where(isNotDieFace).ToArray();
            Assert.Empty(outsideValues);
        }

        [Fact]
        public void Next_DieRange_ProducesEveryFace()
        {
            int[] faces = Enumerable.Range(0, Repetitions)
                .Select(repetition => RollDie())
                .Distinct()
                .ToArray();

            Assert.Equal(DiceHand.MaximumFaceValue - DiceHand.MinimumFaceValue + 1, faces.Length);
        }

        [Fact]
        public void Next_RangeOfOneValue_ReturnsTheMinimum()
        {
            int value = SecureRandom.Next(SingleValue, SingleValue + 1);

            Assert.Equal(SingleValue, value);
        }

        [Fact]
        public void Next_NegativeRange_StaysInsideTheRange()
        {
            int[] values = Enumerable.Range(0, Repetitions)
                .Select(repetition => SecureRandom.Next(NegativeMinimum, NegativeMaximum))
                .ToArray();
            Func<int, bool> isNotInNegativeRange = value => !IsInNegativeRange(value);

            int[] outsideValues = values.Where(isNotInNegativeRange).ToArray();
            Assert.Empty(outsideValues);
        }

        [Theory]
        [InlineData(SingleValue, SingleValue)]
        [InlineData(SingleValue, NegativeMinimum)]
        public void Next_MaximumNotGreaterThanMinimum_ThrowsArgumentOutOfRangeException(int minValue, int maxValue)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SecureRandom.Next(minValue, maxValue));
        }

        private static int RollDie()
        {
            return SecureRandom.Next(DiceHand.MinimumFaceValue, DiceHand.MaximumFaceValue + 1);
        }

        private static bool IsDieFace(int value)
        {
            return value >= DiceHand.MinimumFaceValue && value <= DiceHand.MaximumFaceValue;
        }

        private static bool IsInNegativeRange(int value)
        {
            return value >= NegativeMinimum && value < NegativeMaximum;
        }
    }
}
