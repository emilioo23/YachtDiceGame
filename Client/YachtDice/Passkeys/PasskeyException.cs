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
        public PasskeyException(string message)
            : base(message)
        {
        }
    }
}
