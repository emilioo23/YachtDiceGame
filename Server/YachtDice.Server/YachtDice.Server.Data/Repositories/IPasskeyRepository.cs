using System.Collections.Generic;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Data access operations for the passkeys of the players. Only the public key is saved.
    /// </summary>
    public interface IPasskeyRepository
    {
        /// <summary>
        /// Looks for the passkeys of a player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The passkeys of the player; an empty list if there are none.</returns>
        List<PasskeyRecord> FindByPlayer(int playerId);

        /// <summary>
        /// Looks for the passkey that has the credential identifier.
        /// </summary>
        /// <param name="credentialId">Identifier of the credential, in base64url.</param>
        /// <returns>The passkey, or null if there is none.</returns>
        PasskeyRecord FindByCredentialId(string credentialId);

        /// <summary>
        /// Saves a new passkey.
        /// </summary>
        /// <param name="record">Passkey to save.</param>
        void Add(PasskeyRecord record);

        /// <summary>
        /// Saves the signature counter that the authenticator reported in its last use.
        /// </summary>
        /// <param name="passkeyId">Identifier of the passkey.</param>
        /// <param name="signCount">New value of the counter.</param>
        void SaveSignCount(int passkeyId, long signCount);
    }
}
