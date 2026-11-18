using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// Answer of the authenticator when the player logs in with a passkey. Binary data is written in base64url.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class PasskeyAssertionData
    {
        /// <summary>
        /// Identifier of the challenge that was answered.
        /// </summary>
        [DataMember]
        public string ChallengeId { get; set; }

        /// <summary>
        /// Identifier of the credential that signed.
        /// </summary>
        [DataMember]
        public string CredentialId { get; set; }

        /// <summary>
        /// Client data JSON that the client signed with the authenticator.
        /// </summary>
        [DataMember]
        public string ClientDataJson { get; set; }

        /// <summary>
        /// Authenticator data that the authenticator signed.
        /// </summary>
        [DataMember]
        public string AuthenticatorData { get; set; }

        /// <summary>
        /// Signature of the authenticator data and the hash of the client data.
        /// </summary>
        [DataMember]
        public string Signature { get; set; }

        /// <summary>
        /// Identifier of the player that the authenticator saved with the credential; it may be empty.
        /// </summary>
        [DataMember]
        public string UserHandle { get; set; }
    }
}
