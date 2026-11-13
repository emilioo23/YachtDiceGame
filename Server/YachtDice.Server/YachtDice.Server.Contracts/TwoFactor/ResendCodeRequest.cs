using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// Data needed to send a new code: the challenge the code answers and the language of the email. Both values
    /// always travel together.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class ResendCodeRequest
    {
        /// <summary>
        /// Identifier of the challenge that needs a new code.
        /// </summary>
        [DataMember]
        public string ChallengeId { get; set; }

        /// <summary>
        /// Culture or language code preferred by the player.
        /// </summary>
        [DataMember]
        public string Culture { get; set; }
    }
}
