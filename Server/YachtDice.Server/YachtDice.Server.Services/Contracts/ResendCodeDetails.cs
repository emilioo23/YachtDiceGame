using System.Runtime.Serialization;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Data sent by the client to ask for a new verification code.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class ResendCodeDetails
    {
        /// <summary>
        /// Player's email address.
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Unique challenge identifier tied to the login session.
        /// </summary>
        [DataMember]
        public string ChallengeId { get; set; }

        /// <summary>
        /// Name of the language of the client (for example "es-MX"), used to write the email.
        /// </summary>
        [DataMember]
        public string CultureName { get; set; }
    }
}