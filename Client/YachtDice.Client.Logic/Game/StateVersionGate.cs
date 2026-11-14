namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// Lets the window show only the states of the game that are newer than the last one it showed. The
    /// server sends the states in order, but two notifications can arrive in the opposite order.
    /// </summary>
    public sealed class StateVersionGate
    {
        private const int NoVersion = 0;

        private int _lastVersion = NoVersion;

        /// <summary>
        /// Tells whether a state is newer than the last accepted one, and remembers it if it is.
        /// </summary>
        /// <param name="version">Version of the state that arrived.</param>
        /// <returns>True if the state must be shown; false if it is old or repeated.</returns>
        public bool TryAccept(int version)
        {
            var isNewer = version > _lastVersion;

            if (isNewer)
            {
                _lastVersion = version;
            }

            return isNewer;
        }
    }
}
