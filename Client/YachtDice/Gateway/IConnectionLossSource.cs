using System;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Something that keeps a connection open with the server and tells when that connection is lost without
    /// the application asking to close it.
    /// </summary>
    public interface IConnectionLossSource
    {
        /// <summary>
        /// Event triggered when the connection with the server is lost. It is not triggered when the application
        /// closes the connection itself. It can be raised from any thread.
        /// </summary>
        event Action ConnectionLost;
    }
}
