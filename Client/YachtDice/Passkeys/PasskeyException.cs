using System;

namespace YachtDice.Passkeys
{
    /// <summary>
    /// Thrown when Windows could not create or use a passkey.
    /// </summary>
    public class PasskeyException : Exception
    {
        /// <summary>
        /// Initializes the exception.
        /// </summary>
        /// <param name="message">What failed.</param>
        /// <param name="isCancelled">True if the player closed the Windows dialog or the time ran out.</param>
        public PasskeyException(string message, bool isCancelled)
            : base(message)
        {
            IsCancelled = isCancelled;
        }

        /// <summary>
        /// Gets a value that says whether the player cancelled the operation. It is not an error, so no message
        /// is shown to the player.
        /// </summary>
        public bool IsCancelled { get; }
    }
}
