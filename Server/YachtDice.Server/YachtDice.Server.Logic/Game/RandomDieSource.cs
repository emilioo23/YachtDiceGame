using System;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Rolls dice with a pseudo-random generator. It can be shared by several games because it is
    /// thread-safe.
    /// </summary>
    public sealed class RandomDieSource : IDieSource
    {
        private readonly object _syncRoot = new object();
        private readonly Random _random = new Random();

        /// <inheritdoc />
        public int Roll()
        {
            int value;

            lock (_syncRoot)
            {
                value = _random.Next(DiceRules.MinimumFaceValue, DiceRules.MaximumFaceValue + 1);
            }

            return value;
        }
    }
}
