using System.Runtime.Serialization;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Data sent by the client to validate a verification code.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class VerifyCodeRequest
    {
        /// <summary>
        /// Email address the code was sent to.
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Code entered by the player.
        /// </summary>
        [DataMember]
        public string Code { get; set; }

        /// <summary>
        /// Unique challenge identifier tied to the login session.
        /// </summary>
        [DataMember]
        public string ChallengeId { get; set; }
    }
}