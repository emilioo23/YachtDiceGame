using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Rolls dice with a cryptographically secure generator, so nobody can predict the next values. It keeps
    /// no state, so several games can share it at the same time.
    /// </summary>
    public sealed class RandomDieSource : IDieSource
    {
        /// <inheritdoc />
        public int Roll()
        {
            return SecureRandom.Next(DiceRules.MinimumFaceValue, DiceRules.MaximumFaceValue + 1);
        }
    }
}
