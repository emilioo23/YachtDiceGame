using System.Runtime.Serialization;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Data sent by the client to validate a verification code.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class VerifyCodeRequestDto
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
    }
}