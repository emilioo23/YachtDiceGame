using System;
using YachtDice.Server.Services;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Service without operations of its own that opens the common behavior of the services to the tests:
    /// the check of the session, the check of a guest name and the conversion of a failure into a fault.
    /// </summary>
    internal sealed class InMemoryService : ServiceBase
    {
        /// <summary>
        /// Gets the name of the player who calls, the same way the operations that a guest can use do it.
        /// </summary>
        /// <param name="sessionToken">Token of the session, or an empty text for a guest.</param>
        /// <param name="guestName">Name that the guest wants to show.</param>
        /// <returns>The name of the player who calls.</returns>
        public string FindCaller(string sessionToken, string guestName)
        {
            return Execute(guestName, () => ResolvePlayerName(sessionToken, guestName));
        }

        /// <summary>
        /// Runs an operation for the player of the session.
        /// </summary>
        /// <param name="sessionToken">Token of the session.</param>
        /// <param name="operation">Operation that receives the name of the player of the session.</param>
        /// <returns>The result of the operation.</returns>
        public string RunForPlayer(string sessionToken, Func<string, string> operation)
        {
            return ExecuteForPlayer(sessionToken, operation);
        }

        /// <summary>
        /// Runs an operation that needs data from the client.
        /// </summary>
        /// <param name="data">Data that the client sent.</param>
        /// <param name="operation">Operation to run.</param>
        /// <returns>The result of the operation.</returns>
        public string RunWithData(object data, Func<string> operation)
        {
            return Execute(data, operation);
        }
    }
}
