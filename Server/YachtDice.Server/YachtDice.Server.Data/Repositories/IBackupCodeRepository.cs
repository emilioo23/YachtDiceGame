using System.Collections.Generic;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Data access operations for the backup codes that replace the code sent by email when the player logs in.
    /// Only the hash of each code is saved.
    /// </summary>
    public interface IBackupCodeRepository
    {
        /// <summary>
        /// Deletes the codes that the player had and saves the new ones as active.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="codeHashes">Hashes of the new codes.</param>
        void Replace(int playerId, IEnumerable<string> codeHashes);

        /// <summary>
        /// Marks as used the active code of the player that has the hash.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="codeHash">Hash of the code written by the player.</param>
        /// <returns>True if an active code had that hash and it was marked as used.</returns>
        bool TryUse(int playerId, string codeHash);

        /// <summary>
        /// Counts the codes of the player that have not been used.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The number of active codes.</returns>
        int CountActive(int playerId);

        /// <summary>
        /// Deletes every code of the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        void RemoveAll(int playerId);
    }
}
