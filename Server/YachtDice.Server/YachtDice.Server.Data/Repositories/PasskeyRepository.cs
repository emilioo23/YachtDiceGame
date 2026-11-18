using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using YachtDice.Server.Data.Exceptions;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the data access of the passkeys.
    /// </summary>
    public class PasskeyRepository : IPasskeyRepository
    {
        private const string NullRecordMessage = "The passkey to save cannot be null.";
        private const string QueryFailedMessage = "Database error occurred while reading the passkeys.";
        private const string UpdateFailedMessage = "Database error occurred while saving the passkeys.";

        /// <summary>
        /// Looks for the passkeys of a player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The passkeys of the player; an empty list if there are none.</returns>
        /// <exception cref="DataAccessException">The passkeys could not be read.</exception>
        public List<PasskeyRecord> FindByPlayer(int playerId)
        {
            List<PasskeyRecord> records = DatabaseOperation.Query(
                context => SelectByPlayer(context, playerId),
                QueryFailedMessage);

            return records;
        }

        /// <summary>
        /// Looks for the passkey that has the credential identifier.
        /// </summary>
        /// <param name="credentialId">Identifier of the credential, in base64url.</param>
        /// <returns>The passkey, or null if there is none.</returns>
        /// <exception cref="DataAccessException">The passkey could not be read.</exception>
        public PasskeyRecord FindByCredentialId(string credentialId)
        {
            TextGuard.RejectBlank(credentialId, nameof(credentialId));

            PasskeyRecord record = DatabaseOperation.Query(
                context => SelectByCredentialId(context, credentialId),
                QueryFailedMessage);

            return record;
        }

        /// <summary>
        /// Saves a new passkey.
        /// </summary>
        /// <param name="record">Passkey to save.</param>
        /// <exception cref="ArgumentNullException">The passkey is null.</exception>
        /// <exception cref="DataAccessException">The passkey could not be saved.</exception>
        public void Add(PasskeyRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record), NullRecordMessage);
            }

            DatabaseOperation.Save(context => context.Passkey.Add(CreateEntity(record)), UpdateFailedMessage);
        }

        /// <summary>
        /// Saves the signature counter that the authenticator reported in its last use.
        /// </summary>
        /// <param name="passkeyId">Identifier of the passkey.</param>
        /// <param name="signCount">New value of the counter.</param>
        /// <exception cref="DataAccessException">The counter could not be saved.</exception>
        public void SaveSignCount(int passkeyId, long signCount)
        {
            DatabaseOperation.Save(context => UpdateSignCount(context, passkeyId, signCount), UpdateFailedMessage);
        }

        private static List<PasskeyRecord> SelectByPlayer(YachtDiceContext context, int playerId)
        {
            Expression<Func<Passkey, bool>> isPlayerKey = key => key.PlayerId == playerId;

            return context.Passkey.Include(key => key.Player).Where(isPlayerKey).ToList().Select(ToRecord).ToList();
        }

        private static PasskeyRecord SelectByCredentialId(YachtDiceContext context, string credentialId)
        {
            Expression<Func<Passkey, bool>> hasCredential = key => key.CredentialId == credentialId;
            Passkey entity = context.Passkey.Include(key => key.Player).Where(hasCredential).FirstOrDefault();
            PasskeyRecord record = null;

            if (entity != null)
            {
                record = ToRecord(entity);
            }

            return record;
        }

        private static void UpdateSignCount(YachtDiceContext context, int passkeyId, long signCount)
        {
            Expression<Func<Passkey, bool>> isPasskey = key => key.Id == passkeyId;
            Passkey entity = context.Passkey.Where(isPasskey).FirstOrDefault();

            if (entity != null)
            {
                entity.SignCount = signCount;
            }
        }

        private static Passkey CreateEntity(PasskeyRecord record)
        {
            return new Passkey
            {
                PlayerId = record.PlayerId,
                CredentialId = record.CredentialId,
                PublicKey = record.PublicKey,
                SignCount = record.SignCount,
                DeviceName = record.DeviceName
            };
        }

        private static PasskeyRecord ToRecord(Passkey entity)
        {
            return new PasskeyRecord
            {
                Id = entity.Id,
                PlayerId = entity.PlayerId,
                CredentialId = entity.CredentialId,
                PublicKey = entity.PublicKey,
                SignCount = entity.SignCount,
                DeviceName = entity.DeviceName,
                PlayerDisplayName = entity.Player.DisplayName,
                PlayerFirstName = entity.Player.FirstName
            };
        }
    }
}
