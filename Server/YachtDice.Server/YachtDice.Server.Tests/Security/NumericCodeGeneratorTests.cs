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
            for (var repetition = 0; repetition < Repetitions; repetition++)
            {
                string code = NumericCodeGenerator.Generate(SixDigits);

                Assert.Equal(SixDigits, code.Length);
                Assert.True(code.All(char.IsDigit));
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Generate_LengthBelowOne_ThrowsArgumentOutOfRangeException(int length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => NumericCodeGenerator.Generate(length));
        }
    }
}
