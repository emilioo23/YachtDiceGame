using System;
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
            Func<PasskeyRecord, bool> isOfPlayer = record => record.PlayerId == playerId;

            return _records.Where(isOfPlayer).ToList();
        }

        /// <inheritdoc />
        public PasskeyRecord FindByCredentialId(string credentialId)
        {
            Func<PasskeyRecord, bool> hasCredentialId = record => record.CredentialId == credentialId;

            return _records.FirstOrDefault(hasCredentialId);
        }

        /// <inheritdoc />
        public void Add(PasskeyRecord record)
        {
            Func<Player, bool> isOwner = player => player.Id == record.PlayerId;
            Player owner = _players.Players.First(isOwner);

            record.Id = _nextId;
            record.PlayerDisplayName = owner.DisplayName;
            record.PlayerFirstName = owner.FirstName;
            _nextId++;
            _records.Add(record);
        }

        /// <inheritdoc />
        public void Delete(int passkeyId)
        {
            Predicate<PasskeyRecord> isDeleted = record => record.Id == passkeyId;

            _records.RemoveAll(isDeleted);
        }

        /// <inheritdoc />
        public void SaveSignCount(int passkeyId, long signCount)
        {
            Func<PasskeyRecord, bool> isSaved = record => record.Id == passkeyId;

            _records.First(isSaved).SignCount = signCount;
        }
    }
}
