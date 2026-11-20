using System;

namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Challenge that the server issued and that is waiting for the answer of the authenticator.
    /// </summary>
    public sealed class PendingChallenge
    {
        /// <summary>
        /// Initializes a pending challenge.
        /// </summary>
        /// <param name="id">Identifier that the client sends back with its answer.</param>
        /// <param name="challenge">Random bytes that the authenticator must sign.</param>
        /// <param name="expiresAt">Moment after which the challenge stops working.</param>
        public PendingChallenge(string id, byte[] challenge, DateTime expiresAt)
        {
            Id = id;
            Challenge = challenge;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Identifier that the client sends back with its answer.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Random bytes that the authenticator must sign.
        /// </summary>
        public byte[] Challenge { get; }

        /// <summary>
        /// Moment after which the challenge stops working.
        /// </summary>
        public DateTime ExpiresAt { get; }

        /// <summary>
        /// Purpose of the challenge.
        /// </summary>
        public PasskeyPurpose Purpose { get; set; }

        /// <summary>
        /// Identifier of the player that asked for the challenge; zero when the player is not known yet.
        /// </summary>
        public int PlayerId { get; set; }
    }
}
