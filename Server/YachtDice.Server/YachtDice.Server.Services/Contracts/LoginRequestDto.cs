using System.Runtime.Serialization;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Data sent by the client to start a login.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class LoginRequestDto
    {
        /// <summary>
        /// Player's email address.
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Password in plain text.
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Name of the language of the client (for example "es-MX"), used to write the verification email.
        /// </summary>
        [DataMember]
        public string CultureName { get; set; }
    }
}