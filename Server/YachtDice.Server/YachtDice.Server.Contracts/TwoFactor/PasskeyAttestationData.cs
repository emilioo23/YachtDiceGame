using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// Answer of the authenticator when the player creates a passkey. Binary data is written in base64url.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class PasskeyAttestationData
    {
        /// <summary>
        /// Identifier of the challenge that was answered.
        /// </summary>
        [DataMember]
        public string ChallengeId { get; set; }

        /// <summary>
        /// Client data JSON that the client signed with the authenticator.
        /// </summary>
        [DataMember]
        public string ClientDataJson { get; set; }

        /// <summary>
        /// Attestation object with the authenticator data and the new public key.
        /// </summary>
        [DataMember]
        public string AttestationObject { get; set; }
    }
}
