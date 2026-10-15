using System;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Error raised when a request to the server could not be completed.
    /// </summary>
    public class GatewayException : Exception
    {
        /// <summary>
        /// Initializes the exception without a message.
        /// </summary>
        public GatewayException()
        {
        }

        /// <summary>
        /// Initializes the exception with a message.
        /// </summary>
        /// <param name="message">Description of the error.</param>
        public GatewayException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes the exception with a message and the error that caused it.
        /// </summary>
        /// <param name="message">Description of the error.</param>
        /// <param name="innerException">Error that caused this one.</param>
        public GatewayException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes the exception with the kind of failure, a message and the error that caused it.
        /// </summary>
        /// <param name="errorCode">Kind of failure.</param>
        /// <param name="message">Description of the error.</param>
        /// <param name="innerException">Error that caused this one.</param>
        public GatewayException(GatewayErrorCode errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Kind of failure; the windows use it to choose the message shown to the player.
        /// </summary>
        public GatewayErrorCode ErrorCode { get; }
    }
}