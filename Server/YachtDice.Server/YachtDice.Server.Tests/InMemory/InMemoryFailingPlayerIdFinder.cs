using YachtDice.Server.Data.Exceptions;
using YachtDice.Server.Logic.Friends;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Finder of identifiers that always fails like a database that is not available.
    /// </summary>
    public class InMemoryFailingPlayerIdFinder : IPlayerIdFinder
    {
        private const string UnavailableMessage = "The database is not available.";

        /// <inheritdoc />
        public int FindPlayerId(string displayName)
        {
            throw new DataAccessException(UnavailableMessage);
        }
    }
}
