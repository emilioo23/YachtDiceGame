using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Services.Mapping
{
    using Contracts = YachtDice.Server.Contracts.TwoFactor;
    using LogicPasskeys = YachtDice.Server.Logic.Passkeys;

    /// <summary>
    /// Converts the passkey data between the logic layer and the contracts.
    /// </summary>
    internal static class PasskeyMapper
    {
        private const string NullAttestationMessage = "The answer of the authenticator cannot be null.";

        /// <summary>
        /// Converts the challenge of the logic layer into the contract.
        /// </summary>
        /// <param name="challenge">Challenge of the logic layer.</param>
        /// <returns>The challenge for the client.</returns>
        public static Contracts.PasskeyChallengeData ToContract(LogicPasskeys.PasskeyChallenge challenge)
        {
            return new Contracts.PasskeyChallengeData
            {
                Code = EnumMapper.Map<LogicPasskeys.PasskeyStartStatus, Contracts.PasskeyStartResultCode>(
                    challenge.Status),
                ChallengeId = challenge.ChallengeId,
                Challenge = challenge.Challenge,
                UserHandle = challenge.UserHandle,
                UserName = challenge.UserName,
                ExcludedCredentialIds = challenge.ExcludedCredentialIds.ToList()
            };
        }

        /// <summary>
        /// Converts the passkeys of the logic layer into the contract.
        /// </summary>
        /// <param name="summaries">Passkeys of the logic layer.</param>
        /// <returns>The passkeys for the client.</returns>
        public static List<Contracts.PasskeyInfoData> ToContract(IEnumerable<LogicPasskeys.PasskeySummary> summaries)
        {
            return summaries
                .Select(summary => new Contracts.PasskeyInfoData { Id = summary.Id, Name = summary.Name })
                .ToList();
        }

        /// <summary>
        /// Converts the answer of a registration into the type of the logic layer.
        /// </summary>
        /// <param name="attestation">Answer received from the client.</param>
        /// <returns>The answer for the logic layer.</returns>
        /// <exception cref="ArgumentNullException">The answer is null.</exception>
        public static LogicPasskeys.PasskeyAttestation ToLogic(Contracts.PasskeyAttestationData attestation)
        {
            if (attestation == null)
            {
                throw new ArgumentNullException(nameof(attestation), NullAttestationMessage);
            }

            return new LogicPasskeys.PasskeyAttestation
            {
                ChallengeId = attestation.ChallengeId,
                ClientDataJson = attestation.ClientDataJson,
                AttestationObject = attestation.AttestationObject
            };
        }

        /// <summary>
        /// Converts the answer of a login into the type of the logic layer.
        /// </summary>
        /// <param name="assertion">Answer received from the client.</param>
        /// <returns>The answer for the logic layer.</returns>
        /// <exception cref="ArgumentNullException">The answer is null.</exception>
        public static LogicPasskeys.PasskeyAssertion ToLogic(Contracts.PasskeyAssertionData assertion)
        {
            if (assertion == null)
            {
                throw new ArgumentNullException(nameof(assertion), NullAttestationMessage);
            }

            return new LogicPasskeys.PasskeyAssertion
            {
                ChallengeId = assertion.ChallengeId,
                CredentialId = assertion.CredentialId,
                ClientDataJson = assertion.ClientDataJson,
                AuthenticatorData = assertion.AuthenticatorData,
                Signature = assertion.Signature,
                UserHandle = assertion.UserHandle
            };
        }
    }
}
