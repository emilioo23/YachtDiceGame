using System.Threading;

namespace YachtDice.Client.Logic.Connection
{
    /// <summary>
    /// Lets only one of several reports of the same lost connection be handled. When the server goes down, the
    /// presence, the room and the chat connections break at almost the same time, but the player must be told
    /// once.
    /// </summary>
    public sealed class ConnectionLossGate
    {
        private const int Open = 0;
        private const int Taken = 1;

        private int _state = Open;

        /// <summary>
        /// Tries to take the turn to handle the loss. It works from any thread.
        /// </summary>
        /// <returns>True for the first caller; false while the turn is taken.</returns>
        public bool TryEnter()
        {
            return Interlocked.CompareExchange(ref _state, Taken, Open) == Open;
        }

        /// <summary>
        /// Gives the turn back, so a later loss of the connection is handled again.
        /// </summary>
        public void Release()
        {
            Interlocked.Exchange(ref _state, Open);
        }
    }
}
