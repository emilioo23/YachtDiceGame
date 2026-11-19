using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Authentication
{
    /// <summary>
    /// Represents the credentials required for player authentication.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class LoginCredentials
    {
        /// <summary>
        /// Email address or username of the player.
        /// </summary>
        [DataMember]
        public string Identifier { get; set; }

        /// <summary>
        /// Password in plain text.
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Name of the language of the client (for example "es-MX"), used for verification messages.
        /// </summary>
        [DataMember]
        public string CultureName { get; set; }
    }
}