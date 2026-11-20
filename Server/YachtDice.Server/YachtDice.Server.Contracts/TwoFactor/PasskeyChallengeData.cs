using System.Collections.Generic;
using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// Challenge that the client gives to the authenticator of the player. Binary data is written in base64url.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class PasskeyChallengeData
    {
        /// <summary>
        /// Outcome of asking for the challenge.
        /// </summary>
        [DataMember]
        public PasskeyStartResultCode Code { get; set; }

        /// <summary>
        /// Identifier that the client sends back with the answer of the authenticator.
        /// </summary>
        [DataMember]
        public string ChallengeId { get; set; }

        /// <summary>
        /// Random bytes that the authenticator signs.
        /// </summary>
        [DataMember]
        public string Challenge { get; set; }

        /// <summary>
        /// Identifier of the player for the authenticator; empty for a login.
        /// </summary>
        [DataMember]
        public string UserHandle { get; set; }

        /// <summary>
        /// Name of the player that the authenticator shows; empty for a login.
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        /// Credentials that the player already has, so the authenticator does not create another one in the same
        /// device.
        /// </summary>
        [DataMember]
        public List<string> ExcludedCredentialIds { get; set; }
    }
}
