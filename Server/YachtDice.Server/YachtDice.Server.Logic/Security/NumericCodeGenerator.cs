using System;
using System.Globalization;
using System.Security.Cryptography;

namespace YachtDice.Server.Logic.Security
{
    /// <summary>
    /// Generates random numeric codes with a cryptographically secure generator.
    /// </summary>
    public static class NumericCodeGenerator
    {
        private const int MinimumLength = 1;
        private const int DecimalBase = 10;
        private const int BufferStartIndex = 0;
        private const char PaddingCharacter = '0';
        private const string InvalidLengthMessage = "The code must have at least one digit.";

        /// <summary>
        /// Creates a random code made only of digits.
        /// </summary>
        /// <param name="length">Number of digits of the code.</param>
        /// <returns>The code, padded with zeros on the left when needed.</returns>
        public static string Generate(int length)
        {
            if (length < MinimumLength)
            {
                throw new ArgumentOutOfRangeException(nameof(length), InvalidLengthMessage);
            }

            var upperBound = (long)Math.Pow(DecimalBase, length);
            var buffer = new byte[sizeof(uint)];

            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(buffer);
            }

            uint randomValue = BitConverter.ToUInt32(buffer, BufferStartIndex);

            return (randomValue % upperBound)
                .ToString(CultureInfo.InvariantCulture)
                .PadLeft(length, PaddingCharacter);
        }
    }
}
