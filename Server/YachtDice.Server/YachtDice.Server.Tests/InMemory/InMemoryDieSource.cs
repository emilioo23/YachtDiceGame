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

        public InMemoryDieSource(params int[] values)
        {
            _values = values;
        }

        public int Roll()
        {
            var value = _values[_position % _values.Count];

            _position++;

            return value;
        }
    }
}
