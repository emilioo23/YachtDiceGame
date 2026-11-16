using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Keeps the games in progress in memory so the tests do not need the database.
    /// </summary>
    internal sealed class InMemoryGameCheckpointRepository : IGameCheckpointRepository
    {
        private readonly Dictionary<string, GameCheckpointRecord> _checkpoints =
            new Dictionary<string, GameCheckpointRecord>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets how many times a game was saved.
        /// </summary>
        public int SaveCount { get; private set; }

        /// <summary>
        /// Gets the saved game of a room.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>The saved game, or null if the room has none.</returns>
        public GameCheckpointRecord Find(string roomCode)
        {
            _checkpoints.TryGetValue(roomCode, out GameCheckpointRecord checkpoint);

            return checkpoint;
        }

        /// <inheritdoc />
        public void Save(GameCheckpointRecord checkpoint)
        {
            _checkpoints[checkpoint.RoomCode] = checkpoint;
            SaveCount++;
        }

        /// <inheritdoc />
        public List<GameCheckpointRecord> FindAll()
        {
            return _checkpoints.Values.ToList();
        }

        /// <inheritdoc />
        public void Remove(string roomCode)
        {
            _checkpoints.Remove(roomCode);
        }
    }
}
