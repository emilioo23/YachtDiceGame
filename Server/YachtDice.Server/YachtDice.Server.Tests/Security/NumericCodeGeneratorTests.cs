using System;
using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Tests.Security
{
    /// <summary>
    /// Random numeric codes used for the verification and the password recovery.
    /// </summary>
    public class NumericCodeGeneratorTests
    {
        private const int SixDigits = 6;
        private const int Repetitions = 50;
        private const string LengthParameterName = "length";

        [Fact]
        public void Generate_SixDigits_ReturnsOnlyDigitsWithTheRequestedLength()
        {
            string[] codes = Enumerable.Range(0, Repetitions)
                .Select(repetition => NumericCodeGenerator.Generate(SixDigits))
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
            Assert.Throws<ArgumentOutOfRangeException>(() => NumericCodeGenerator.Generate(length));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(9)]
        [InlineData(10)]
        public void Generate_LengthFromOneToTen_ReturnsOnlyDigitsWithTheRequestedLength(int length)
        {
            string code = NumericCodeGenerator.Generate(length);

            Assert.True(IsDigitsOfLength(code, length));
        }

        [Fact]
        public void Generate_MinimumInteger_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => NumericCodeGenerator.Generate(int.MinValue));
        }

        [Fact]
        public void Generate_ZeroDigits_ReportsTheLengthParameter()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => NumericCodeGenerator.Generate(0));

            Assert.Equal(LengthParameterName, exception.ParamName);
        }

        private static bool IsDigitsOfLength(string code, int length)
        {
            return code.Length == length && code.All(char.IsDigit);
        }

        private static bool IsSixDigitCode(string code)
        {
            return code.Length == SixDigits && code.All(char.IsDigit);
        }
    }
}
