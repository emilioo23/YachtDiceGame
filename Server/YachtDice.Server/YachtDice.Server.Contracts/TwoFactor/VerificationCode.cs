using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// Code typed by the player together with the challenge it answers. Both values always travel together,
    /// because a code is only valid for the challenge that created it.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class VerificationCode
    {
        /// <summary>
        /// Code received by email and typed by the player.
        /// </summary>
        [DataMember]
        public string Code { get; set; }

        /// <summary>
        /// Identifier of the challenge the code belongs to.
        /// </summary>
        [DataMember]
        public string ChallengeId { get; set; }
    }
}
