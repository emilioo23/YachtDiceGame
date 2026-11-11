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

        private static bool IsSixDigitCode(string code)
        {
            return code.Length == SixDigits && code.All(char.IsDigit);
        }
    }
}
