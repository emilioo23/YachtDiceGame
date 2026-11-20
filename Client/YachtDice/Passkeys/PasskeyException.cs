using System;

namespace YachtDice.Passkeys
{
    /// <summary>
    /// Thrown when Windows could not create or use a passkey.
    /// </summary>
    public class PasskeyException : Exception
    {
        /// <summary>
        /// Initializes the exception without a message.
        /// </summary>
        public PasskeyException()
        {
        }

        /// <summary>
        /// Initializes the exception.
        /// </summary>
        /// <param name="message">What failed.</param>
        public PasskeyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes the exception with the failure that caused it.
        /// </summary>
        /// <param name="message">What failed.</param>
        /// <param name="innerException">The original failure.</param>
        public PasskeyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
