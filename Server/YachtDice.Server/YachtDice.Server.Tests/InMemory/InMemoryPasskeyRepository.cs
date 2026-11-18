using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Keeps the passkeys in memory so the tests do not need the database.
    /// </summary>
    internal sealed class InMemoryPasskeyRepository : IPasskeyRepository
    {
        private readonly InMemoryPlayerRepository _players;
        private readonly List<PasskeyRecord> _records = new List<PasskeyRecord>();
        private int _nextId = 1;

        /// <summary>
        /// Initializes the repository with the players that own the passkeys.
        /// </summary>
        /// <param name="players">Players of the test.</param>
        public InMemoryPasskeyRepository(InMemoryPlayerRepository players)
        {
            _players = players;
        }

        /// <summary>
        /// Gets the passkeys saved so far.
        /// </summary>
        public IReadOnlyList<PasskeyRecord> Records => _records;

        /// <inheritdoc />
        public List<PasskeyRecord> FindByPlayer(int playerId)
        {
            return _records.Where(record => record.PlayerId == playerId).ToList();
        }

        /// <inheritdoc />
        public PasskeyRecord FindByCredentialId(string credentialId)
        {
            return _records.FirstOrDefault(record => record.CredentialId == credentialId);
        }

        /// <inheritdoc />
        public void Add(PasskeyRecord record)
        {
            Player owner = _players.Players.First(player => player.Id == record.PlayerId);

            record.Id = _nextId;
            record.PlayerDisplayName = owner.DisplayName;
            record.PlayerFirstName = owner.FirstName;
            _nextId++;
            _records.Add(record);
        }

        /// <inheritdoc />
        public void SaveSignCount(int passkeyId, long signCount)
        {
            _records.First(record => record.Id == passkeyId).SignCount = signCount;
        }
    }
}
