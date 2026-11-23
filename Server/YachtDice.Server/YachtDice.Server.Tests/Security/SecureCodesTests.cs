using System;
using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Tests.Security
{
    /// <summary>
    /// Secure random numbers and numeric codes of the verification.
    /// </summary>
    public class SecureCodesTests
    {
        private const int SixDigits = 6;
        private const int Repetitions = 50;
        private const int RandomRepetitions = 600;
        private const string LengthParameterName = "length";
        private const int NoDigits = 0;
        private const int SingleValue = 7;
        private const int NegativeMinimum = -3;
        private const int NegativeMaximum = 3;

        [Fact]
        public void Generate_SixDigits_ReturnsOnlyDigitsWithTheRequestedLength()
        {
            string[] codes = Enumerable.Range(0, Repetitions)
                .Select(repetition => SecureCodes.Generate(SixDigits))
                .ToArray();
            Func<string, bool> isNotSixDigitCode = code => !IsSixDigitCode(code);

            string[] invalidCodes = codes.Where(isNotSixDigitCode).ToArray();
            Assert.Empty(invalidCodes);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Generate_LengthBelowOne_ThrowsArgumentOutOfRangeException(int length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SecureCodes.Generate(length));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(9)]
        [InlineData(10)]
        public void Generate_LengthFromOneToTen_ReturnsOnlyDigitsWithTheRequestedLength(int length)
        {
            string code = SecureCodes.Generate(length);

            Assert.True(IsDigitsOfLength(code, length));
        }

        [Fact]
        public void Generate_MinimumInteger_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SecureCodes.Generate(int.MinValue));
        }

        [Fact]
        public void Generate_ZeroDigits_ReportsTheLengthParameter()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                LengthParameterName,
                () => SecureCodes.Generate(NoDigits));
        }

        private static bool IsDigitsOfLength(string code, int length)
        {
            return code.Length == length && code.All(char.IsDigit);
        }

        private static bool IsSixDigitCode(string code)
        {
            return code.Length == SixDigits && code.All(char.IsDigit);
        }

        [Fact]
        public void Next_DieRange_StaysInsideTheRange()
        {
            int[] values = Enumerable.Range(0, RandomRepetitions)
                .Select(repetition => RollDie())
                .ToArray();
            Func<int, bool> isNotDieFace = value => !IsDieFace(value);

            int[] outsideValues = values.Where(isNotDieFace).ToArray();
            Assert.Empty(outsideValues);
        }

        [Fact]
        public void Next_DieRange_ProducesEveryFace()
        {
            int[] faces = Enumerable.Range(0, RandomRepetitions)
                .Select(repetition => RollDie())
                .Distinct()
                .ToArray();

            Assert.Equal(DiceHand.MaximumFaceValue - DiceHand.MinimumFaceValue + 1, faces.Length);
        }

        [Fact]
        public void Next_RangeOfOneValue_ReturnsTheMinimum()
        {
            int value = SecureCodes.Next(SingleValue, SingleValue + 1);

            Assert.Equal(SingleValue, value);
        }

        [Fact]
        public void Next_NegativeRange_StaysInsideTheRange()
        {
            int[] values = Enumerable.Range(0, RandomRepetitions)
                .Select(repetition => SecureCodes.Next(NegativeMinimum, NegativeMaximum))
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
            Assert.Throws<ArgumentOutOfRangeException>(() => SecureCodes.Next(minValue, maxValue));
        }

        private static int RollDie()
        {
            return SecureCodes.Next(DiceHand.MinimumFaceValue, DiceHand.MaximumFaceValue + 1);
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
