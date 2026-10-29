using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Data sent by the client to register a new player.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class Player
    {
        /// <summary>
        /// Player's first name.
        /// </summary>
        [DataMember]
        public string FirstName { get; set; }

        /// <summary>
        /// Player's last name.
        /// </summary>
        [DataMember]
        public string LastName { get; set; }

        /// <summary>
        /// Username chosen by the player.
        /// </summary>
        [DataMember]
        public string Username { get; set; }

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
    }
}
