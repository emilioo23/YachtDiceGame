using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using YachtDice.Server.Data.Exceptions;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the data access of the password recovery codes.
    /// </summary>
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private const string FindFailedMessage = "Database error occurred while searching the recovery code.";
        private const string SaveFailedMessage = "Could not save the recovery code due to a database failure.";
        private const string DeleteFailedMessage = "Could not delete the recovery code due to a database failure.";
        private const string CompleteFailedMessage = "Could not save the new password due to a database failure.";
        private const string EmptyValueMessage = "The value cannot be empty.";
        private const string NullTokenMessage = "The recovery code cannot be null.";

        /// <summary>
        /// Looks for the recovery code saved for the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>A list with the saved code, or an empty list if the player has none.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<PwdResetToken> FindByPlayerId(int playerId)
        {
            List<PwdResetToken> tokens = DatabaseOperation.Query(
                context => SelectByPlayer(context, playerId),
                FindFailedMessage);

            return tokens;
        }

        /// <summary>
        /// Saves the recovery code of the player. A previous code of the same player is discarded.
        /// </summary>
        /// <param name="token">Recovery code to save, with its player, its hash and its expiration date.</param>
        /// <exception cref="ArgumentNullException">The token is null.</exception>
        /// <exception cref="ArgumentException">The hash of the token is empty.</exception>
        /// <exception cref="DataAccessException">The code could not be saved.</exception>
        public void Save(PwdResetToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token), NullTokenMessage);
            }

            if (string.IsNullOrWhiteSpace(token.Token))
            {
                throw new ArgumentException(EmptyValueMessage, nameof(token.Token));
            }

            DatabaseOperation.Save(context => ReplaceToken(context, token), SaveFailedMessage);
        }

        /// <summary>
        /// Deletes the recovery code of the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <exception cref="DataAccessException">The code could not be deleted.</exception>
        public void Delete(int playerId)
        {
            DatabaseOperation.Save(context => RemoveTokens(context, playerId), DeleteFailedMessage);
        }

        /// <summary>
        /// Saves the new password of the player and deletes its recovery code in the same operation.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="newPasswordHash">Hash of the new password.</param>
        /// <exception cref="DataAccessException">The new password could not be saved.</exception>
        public void CompleteReset(int playerId, string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
            {
                throw new ArgumentException(EmptyValueMessage, nameof(newPasswordHash));
            }

            DatabaseOperation.Save(
                context => ApplyNewPassword(context, playerId, newPasswordHash),
                CompleteFailedMessage);
        }

        private static List<PwdResetToken> SelectByPlayer(YachtDiceContext context, int playerId)
        {
            Expression<Func<PwdResetToken, bool>> isSamePlayerId = token => token.PlayerId == playerId;
            List<PwdResetToken> tokens = context.PwdResetToken.Where(isSamePlayerId).ToList();

            return tokens;
        }

        private static void RemoveTokens(YachtDiceContext context, int playerId)
        {
            Expression<Func<PwdResetToken, bool>> isSamePlayerId = token => token.PlayerId == playerId;
            context.PwdResetToken.RemoveRange(context.PwdResetToken.Where(isSamePlayerId));
        }

        private static void ReplaceToken(YachtDiceContext context, PwdResetToken token)
        {
            RemoveTokens(context, token.PlayerId);
            context.PwdResetToken.Add(token);
        }

        private static void ApplyNewPassword(YachtDiceContext context, int playerId, string newPasswordHash)
        {
            Expression<Func<Player, bool>> isSameId = row => row.Id == playerId;
            Player player = context.Player.First(isSameId);

            player.PwdHash = newPasswordHash;
            RemoveTokens(context, playerId);
        }
    }
}
