using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Keeps the backup codes in memory so the tests do not need the database.
    /// </summary>
    internal sealed class InMemoryBackupCodeRepository : IBackupCodeRepository
    {
        private readonly List<BackupCode> _codes = new List<BackupCode>();

        /// <summary>
        /// Gets the backup codes saved so far, with their hash and status.
        /// </summary>
        public IReadOnlyList<BackupCode> Codes => _codes;

        /// <inheritdoc />
        public void Replace(int playerId, IEnumerable<string> codeHashes)
        {
            RemoveAll(playerId);

            foreach (string hash in codeHashes)
            {
                _codes.Add(new BackupCode { PlayerId = playerId, Code = hash, Status = BackupCodeStatus.Active });
            }
        }

        /// <inheritdoc />
        public bool TryUse(int playerId, string codeHash)
        {
            Func<BackupCode, bool> isActiveMatch = backupCode =>
                IsActiveCodeOf(backupCode, playerId) && backupCode.Code == codeHash;
            BackupCode code = _codes.FirstOrDefault(isActiveMatch);
            bool isFound = code != null;

            if (isFound)
            {
                code.Status = BackupCodeStatus.Used;
            }

            return isFound;
        }

        /// <inheritdoc />
        public int CountActive(int playerId)
        {
            Func<BackupCode, bool> isActive = backupCode => IsActiveCodeOf(backupCode, playerId);

            return _codes.Count(isActive);
        }

        /// <inheritdoc />
        public void RemoveAll(int playerId)
        {
            Predicate<BackupCode> isPlayerCode = backupCode => backupCode.PlayerId == playerId;

            _codes.RemoveAll(isPlayerCode);
        }

        private static bool IsActiveCodeOf(BackupCode backupCode, int playerId)
        {
            return backupCode.PlayerId == playerId && backupCode.Status == BackupCodeStatus.Active;
        }
    }
}
