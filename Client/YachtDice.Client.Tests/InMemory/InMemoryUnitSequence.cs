using System;

namespace YachtDice.Client.Tests.InMemory
{
    /// <summary>
    /// Random numbers for the tests: it gives the same values again and again, in order, so a test knows which
    /// places the dice try.
    /// </summary>
    internal sealed class InMemoryUnitSequence
    {
        private const string NoValuesMessage = "The sequence needs at least one value.";

        private readonly double[] _values;
        private int _position;

        /// <summary>
        /// Initializes the sequence with the values to repeat.
        /// </summary>
        /// <param name="values">Values between zero and one, in the order they are given.</param>
        /// <exception cref="ArgumentException">There are no values.</exception>
        public InMemoryUnitSequence(params double[] values)
        {
            if (values == null || values.Length == 0)
            {
                throw new ArgumentException(NoValuesMessage, nameof(values));
            }

            _values = values;
        }

        /// <summary>
        /// Gives the next value and starts again from the first one after the last.
        /// </summary>
        /// <returns>The next value of the sequence.</returns>
        public double Next()
        {
            double value = _values[_position % _values.Length];

            _position++;

            return value;
        }
    }
}
