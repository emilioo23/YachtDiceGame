using System.Collections.Generic;
using YachtDice.Server.Logic.Game;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Die source that gives fixed values in order and starts again when it reaches the end, so the tests
    /// know the exact dice of every roll.
    /// </summary>
    internal sealed class InMemoryDieSource : IDieSource
    {
        private readonly IReadOnlyList<int> _values;
        private int _position;

        /// <summary>
        /// Initializes the source with the values that the dice will show, in order.
        /// </summary>
        /// <param name="values">Values of the dice; they start again from the first one when they run out.</param>
        public InMemoryDieSource(params int[] values)
        {
            _values = values;
        }

        /// <inheritdoc />
        public int Roll()
        {
            var value = _values[_position % _values.Count];

            _position++;

            return value;
        }
    }
}
