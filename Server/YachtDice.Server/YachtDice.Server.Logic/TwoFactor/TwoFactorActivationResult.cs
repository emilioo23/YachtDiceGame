using System;
namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Result of the request to turn on the two-step verification.
    /// </summary>
    public class TwoFactorActivationResult
    {
        private const string NullChallengeIdMessage = "The identifier of the challenge cannot be null.";
        private const string NullMaskedEmailMessage = "The masked email cannot be null.";

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoFactorActivationResult"/> class.
        /// </summary>
        /// <param name="status">Outcome of the request.</param>
        /// <param name="challengeId">Identifier of the challenge the code was sent for.</param>
        /// <param name="maskedEmail">Email where the code was sent, with part of it hidden.</param>
        public TwoFactorActivationResult(TwoFactorActivationStatus status, string challengeId, string maskedEmail)
        {
            Status = status;
            ChallengeId = challengeId ?? throw new ArgumentNullException(nameof(challengeId), NullChallengeIdMessage);
            MaskedEmail = maskedEmail ?? throw new ArgumentNullException(nameof(maskedEmail), NullMaskedEmailMessage);
        }

        /// <summary>
        /// Outcome of the request.
        /// </summary>
        public TwoFactorActivationStatus Status { get; }

        /// <summary>
        /// Identifier of the challenge; empty if no code was sent.
        /// </summary>
        public string ChallengeId { get; }

        /// <summary>
        /// Email where the code was sent, with part of it hidden; empty if no code was sent.
        /// </summary>
        public string MaskedEmail { get; }
    }
}
