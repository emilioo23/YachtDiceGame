using System;
using System.Security.Cryptography;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses random numbers with a cryptographically secure generator. The client uses it for the confetti
    /// of the Yacht celebration, and every value of the range has the same probability, because the values
    /// that would favor some numbers are discarded and drawn again.
    /// </summary>
    public sealed class SecureRandom
    {
        private const int BufferStartIndex = 0;
        private const int DoubleBits = 53;
        private const int UnsignedLongBits = 64;
        private const int DiscardedBits = UnsignedLongBits - DoubleBits;
        private const double DoubleScale = 1.0 / (1UL << DoubleBits);
        private const ulong GeneratedValueCount = (ulong)uint.MaxValue + 1;
        private const string InvalidMaximumMessage = "The maximum value must be greater than zero.";

        /// <summary>
        /// Gets a random integer from zero up to a maximum that is not included.
        /// </summary>
        /// <param name="maxExclusive">Value above the largest one that can be returned.</param>
        /// <returns>A value greater than or equal to zero and smaller than the maximum.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The maximum is not greater than zero.</exception>
        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), InvalidMaximumMessage);
            }

            ulong range = (ulong)maxExclusive;
            ulong acceptedLimit = GeneratedValueCount - (GeneratedValueCount % range);
            ulong candidate;

            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                do
                {
                    candidate = NextUnsignedInteger(generator);
                }
                while (candidate >= acceptedLimit);
            }

            return (int)(candidate % range);
        }

        /// <summary>
        /// Gets a random number from zero up to one that is not included.
        /// </summary>
        /// <returns>A value greater than or equal to zero and smaller than one.</returns>
        public double NextDouble()
        {
            var buffer = new byte[sizeof(ulong)];

            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(buffer);
            }

            ulong value = BitConverter.ToUInt64(buffer, BufferStartIndex);

            return (value >> DiscardedBits) * DoubleScale;
        }

        private static ulong NextUnsignedInteger(RandomNumberGenerator generator)
        {
            var buffer = new byte[sizeof(uint)];

            generator.GetBytes(buffer);

            return BitConverter.ToUInt32(buffer, BufferStartIndex);
        }
    }
}
