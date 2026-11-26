using System;
using System.Globalization;
using System.Security.Cryptography;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Logic.Security
{
    /// <summary>
    /// Secure random numbers and the numeric codes of the verification.
    /// </summary>
    public static class SecureCodes
    {
        /// <summary>
        /// Number of digits of a code.
        /// </summary>
        public const int CodeLength = 6;

        /// <summary>
        /// Minutes that a code can be used after it is sent.
        /// </summary>
        public const int ExpirationMinutes = 10;

        /// <summary>
        /// Wrong codes that can be entered before the code stops working.
        /// </summary>
        public const int MaxFailedAttempts = 3;

        /// <summary>
        /// Seconds that the player must wait before asking for a new code.
        /// </summary>
        public const int ResendCooldownSeconds = 30;

        private const int MinimumLength = 1;
        private const int DecimalBase = 10;
        private const int BufferStartIndex = 0;
        private const char PaddingCharacter = '0';
        private const ulong GeneratedValueCount = (ulong)uint.MaxValue + 1;

        /// <summary>
        /// Creates a random code made only of digits.
        /// </summary>
        /// <param name="length">Number of digits of the code.</param>
        /// <returns>The code, padded with zeros on the left when needed.</returns>
        public static string Generate(int length)
        {
            if (length < MinimumLength)
            {
                throw new ArgumentOutOfRangeException(nameof(length), LogMessages.InvalidLengthMessage);
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

        /// <summary>
        /// Gets a random integer inside a range.
        /// </summary>
        /// <param name="minValue">Smallest value that can be returned.</param>
        /// <param name="maxExclusive">Value above the largest one that can be returned.</param>
        /// <returns>A value greater than or equal to the minimum and smaller than the maximum.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The maximum is not greater than the minimum.</exception>
        public static int Next(int minValue, int maxExclusive)
        {
            if (maxExclusive <= minValue)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), LogMessages.InvalidRangeMessage);
            }

            var range = (ulong)((long)maxExclusive - minValue);
            ulong acceptedLimit = GeneratedValueCount - (GeneratedValueCount % range);
            ulong candidate;

            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                do
                {
                    candidate = NextUnsigned(generator);
                }
                while (candidate >= acceptedLimit);
            }

            return (int)(minValue + (long)(candidate % range));
        }

        /// <summary>
        /// Reads four random bytes from the generator as an unsigned number.
        /// </summary>
        /// <param name="generator">Secure generator that is open.</param>
        /// <returns>A number between zero and the largest unsigned 32-bit value.</returns>
        private static ulong NextUnsigned(RandomNumberGenerator generator)
        {
            var buffer = new byte[sizeof(uint)];

            generator.GetBytes(buffer);

            return BitConverter.ToUInt32(buffer, BufferStartIndex);
        }
    }
}
