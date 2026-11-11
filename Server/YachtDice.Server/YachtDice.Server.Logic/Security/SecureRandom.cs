using System;
using System.Security.Cryptography;

namespace YachtDice.Server.Logic.Security
{
    /// <summary>
    /// Chooses random integers with a cryptographically secure generator, so nobody can predict the dice of a
    /// game or the code of a room. Every value of the range has the same probability, because the values that
    /// would favor some numbers are discarded and drawn again.
    /// </summary>
    public static class SecureRandom
    {
        private const int BufferStartIndex = 0;
        private const ulong GeneratedValueCount = (ulong)uint.MaxValue + 1;
        private const string InvalidRangeMessage = "The maximum value must be greater than the minimum value.";

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
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), InvalidRangeMessage);
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
