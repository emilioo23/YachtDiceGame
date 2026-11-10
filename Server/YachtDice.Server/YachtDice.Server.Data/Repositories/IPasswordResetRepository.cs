using System;
using System.Collections.Generic;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Data access operations for the password recovery codes.
    /// </summary>
    public interface IPasswordResetRepository
    {
        /// <summary>
        /// Looks for the recovery code saved for the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>A list with the saved code, or an empty list if the player has none.</returns>
        List<PwdResetToken> FindByPlayerId(int playerId);

        /// <summary>
        /// Saves the recovery code of the player. A previous code of the same player is discarded.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="tokenHash">Hash of the recovery code.</param>
        /// <param name="expireDate">Moment in which the code stops being valid.</param>
        void Save(int playerId, string tokenHash, DateTime expireDate);

        /// <summary>
        /// Deletes the recovery code of the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        void Delete(int playerId);

        /// <summary>
        /// Saves the new password of the player and deletes its recovery code in the same operation.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="newPasswordHash">Hash of the new password.</param>
        void CompleteReset(int playerId, string newPasswordHash);
    }
}
