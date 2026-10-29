using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Answer of the server to the request to turn on the two-step verification.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class TwoFactorActivationData
    {
        /// <summary>
        /// Outcome of the request.
        /// </summary>
        [DataMember]
        public TwoFactorActivationResultCode Code { get; set; }

        /// <summary>
        /// Unique challenge identifier of the verification code that was sent.
        /// </summary>
        [DataMember]
        public string ChallengeId { get; set; }

        /// <summary>
        /// Email address where the code was sent, with part of it hidden.
        /// </summary>
        [DataMember]
        public string MaskedEmail { get; set; }
    }
}
