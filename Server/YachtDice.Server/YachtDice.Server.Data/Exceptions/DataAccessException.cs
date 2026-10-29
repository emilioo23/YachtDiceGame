using System;

namespace YachtDice.Server.Data.Exceptions
{
    /// <summary>
    /// Raised when the data layer cannot complete an operation against the database.
    /// </summary>
    public class DataAccessException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the DataAccessException class.
        /// </summary>
        /// <param name="message">Technical description of the failure, only useful for the logs.</param>
        public DataAccessException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DataAccessException class with the original failure.
        /// </summary>
        /// <param name="message">Technical description of the failure, only useful for the logs.</param>
        /// <param name="innerException">The exception thrown by the database provider.</param>
        public DataAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}