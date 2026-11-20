using System.Collections.Generic;

namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Challenge that the client gives to the authenticator, with the data that the ceremony needs.
    /// </summary>
    public sealed class PasskeyChallenge
    {
        /// <summary>
        /// Initializes a challenge that was not issued.
        /// </summary>
        /// <param name="status">Reason why the challenge was not issued.</param>
        public PasskeyChallenge(PasskeyStartStatus status)
        {
            Status = status;
            ChallengeId = string.Empty;
            Challenge = string.Empty;
            UserHandle = string.Empty;
            UserName = string.Empty;
            ExcludedCredentialIds = new List<string>();
        }

        /// <summary>
        /// Outcome of asking for the challenge.
        /// </summary>
        public PasskeyStartStatus Status { get; set; }

        /// <summary>
        /// Identifier that the client sends back with the answer.
        /// </summary>
        public string ChallengeId { get; set; }

        /// <summary>
        /// Random bytes that the authenticator signs, in base64url.
        /// </summary>
        public string Challenge { get; set; }

        /// <summary>
        /// Identifier of the player for the authenticator, in base64url; empty for a login.
        /// </summary>
        public string UserHandle { get; set; }

        /// <summary>
        /// Name of the player that the authenticator shows; empty for a login.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Credentials that the player already has, so the authenticator does not create another one in the same
        /// device.
        /// </summary>
        public IList<string> ExcludedCredentialIds { get; set; }
    }
}
