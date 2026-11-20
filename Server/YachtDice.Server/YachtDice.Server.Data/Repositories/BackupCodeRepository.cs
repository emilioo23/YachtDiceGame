using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using YachtDice.Server.Data.Exceptions;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the data access of the backup codes.
    /// </summary>
    public class BackupCodeRepository : IBackupCodeRepository
    {
        private const string NullHashesMessage = "The hashes of the backup codes cannot be null.";
        private const string QueryFailedMessage = "Database error occurred while reading the backup codes.";
        private const string UpdateFailedMessage = "Database error occurred while saving the backup codes.";
        private const int NoCodeId = 0;

        /// <summary>
        /// Deletes the codes that the player had and saves the new ones as active.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="codeHashes">Hashes of the new codes.</param>
        /// <exception cref="ArgumentNullException">The hashes are null.</exception>
        /// <exception cref="DataAccessException">The codes could not be saved.</exception>
        public void Replace(int playerId, IEnumerable<string> codeHashes)
        {
            if (codeHashes == null)
            {
                throw new ArgumentNullException(nameof(codeHashes), NullHashesMessage);
            }

            List<string> hashes = codeHashes.ToList();

            Save(context => ReplaceCodes(context, playerId, hashes));
        }

        /// <summary>
        /// Marks as used the active code of the player that has the hash.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="codeHash">Hash of the code written by the player.</param>
        /// <returns>True if an active code had that hash and it was marked as used.</returns>
        /// <exception cref="DataAccessException">The codes could not be read or saved.</exception>
        public bool TryUse(int playerId, string codeHash)
        {
            int codeId = Query(context => FindActiveCodeId(context, playerId, codeHash));
            bool isFound = codeId != NoCodeId;

            if (isFound)
            {
                Save(context => MarkAsUsed(context, codeId));
            }

            return isFound;
        }

        /// <summary>
        /// Counts the codes of the player that have not been used.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The number of active codes.</returns>
        /// <exception cref="DataAccessException">The codes could not be read.</exception>
        public int CountActive(int playerId)
        {
            int activeCount = Query(context => CountActiveCodes(context, playerId));

            return activeCount;
        }

        /// <summary>
        /// Deletes every code of the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <exception cref="DataAccessException">The codes could not be deleted.</exception>
        public void RemoveAll(int playerId)
        {
            Save(context => DeleteCodes(context, playerId));
        }

        private static void ReplaceCodes(YachtDiceContext context, int playerId, List<string> hashes)
        {
            DeleteCodes(context, playerId);

            foreach (string hash in hashes)
            {
                context.BackupCode.Add(new BackupCode
                {
                    PlayerId = playerId,
                    Code = hash,
                    Status = BackupCodeStatus.Active
                });
            }
        }

        private static void DeleteCodes(YachtDiceContext context, int playerId)
        {
            Expression<Func<BackupCode, bool>> isPlayerCode = backupCode => backupCode.PlayerId == playerId;
            List<BackupCode> codes = context.BackupCode.Where(isPlayerCode).ToList();

            context.BackupCode.RemoveRange(codes);
        }

        private static int FindActiveCodeId(YachtDiceContext context, int playerId, string codeHash)
        {
            Expression<Func<BackupCode, bool>> isActivePlayerCode = backupCode =>
                backupCode.PlayerId == playerId && backupCode.Status == BackupCodeStatus.Active;
            Expression<Func<BackupCode, bool>> hasHash = backupCode => backupCode.Code == codeHash;
            BackupCode code = context.BackupCode.Where(isActivePlayerCode).FirstOrDefault(hasHash);
            int codeId = NoCodeId;

            if (code != null)
            {
                codeId = code.Id;
            }

            return codeId;
        }

        private static void MarkAsUsed(YachtDiceContext context, int codeId)
        {
            BackupCode code = context.BackupCode.Find(codeId);

            if (code != null)
            {
                code.Status = BackupCodeStatus.Used;
            }
        }

        private static int CountActiveCodes(YachtDiceContext context, int playerId)
        {
            Expression<Func<BackupCode, bool>> isActivePlayerCode = backupCode =>
                backupCode.PlayerId == playerId && backupCode.Status == BackupCodeStatus.Active;

            return context.BackupCode.Count(isActivePlayerCode);
        }

        private static TResult Query<TResult>(Func<YachtDiceContext, TResult> query)
        {
            return DatabaseOperation.Query(query, QueryFailedMessage);
        }

        private static void Save(Action<YachtDiceContext> change)
        {
            DatabaseOperation.Save(change, UpdateFailedMessage);
        }
    }
}
