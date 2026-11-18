using System.Collections.Generic;
using YachtDice.Server.Logic.Authentication;

namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Defines the business logic of the passkeys: registering them and using them to log in.
    /// </summary>
    public interface IPasskeyService
    {
        /// <summary>
        /// Issues the challenge to create a passkey for a player who is logged in.
        /// </summary>
        /// <param name="displayName">Name of the player of the session.</param>
        /// <returns>The challenge and the data of the player.</returns>
        PasskeyChallenge BeginRegistration(string displayName);

        /// <summary>
        /// Checks the answer of the authenticator and saves the new passkey.
        /// </summary>
        /// <param name="displayName">Name of the player of the session.</param>
        /// <param name="attestation">Answer of the authenticator.</param>
        /// <returns>Outcome of the registration.</returns>
        PasskeyRegistrationStatus CompleteRegistration(string displayName, PasskeyAttestation attestation);

        /// <summary>
        /// Counts the passkeys of a player.
        /// </summary>
        /// <param name="displayName">Name of the player of the session.</param>
        /// <returns>The number of passkeys; zero if the player does not exist.</returns>
        int CountPasskeys(string displayName);

        /// <summary>
        /// Issues the challenge to log in with a passkey. The player is not known yet.
        /// </summary>
        /// <returns>The challenge.</returns>
        /// <summary>
        /// Lists the passkeys of a player.
        /// </summary>
        /// <param name="displayName">Name of the player of the session.</param>
        /// <returns>The passkeys; empty if the player does not exist.</returns>
        List<PasskeySummary> ListPasskeys(string displayName);

        /// <summary>
        /// Removes a passkey of the player after checking the password.
        /// </summary>
        /// <param name="displayName">Name of the player of the session.</param>
        /// <param name="passkeyId">Identifier of the passkey to remove.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the removal.</returns>
        PasskeyRemovalStatus RemovePasskey(string displayName, int passkeyId, string password);

        PasskeyChallenge BeginLogin();

        /// <summary>
        /// Checks the answer of the authenticator and, if it is valid, completes the login.
        /// </summary>
        /// <param name="assertion">Answer of the authenticator.</param>
        /// <returns>Result of the login attempt.</returns>
        LoginResult CompleteLogin(PasskeyAssertion assertion);
    }
}
