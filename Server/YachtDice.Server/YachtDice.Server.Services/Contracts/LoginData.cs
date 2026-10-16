using System.Runtime.Serialization;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Answer of the server to the first step of the login.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class LoginData
    {
        /// <summary>
        /// Outcome of the login attempt.
        /// </summary>
        [DataMember]
        public LoginResultCode Code { get; set; }

        /// <summary>
        /// Player's name; empty if the login did not succeed.
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// Unique challenge identifier tied to the login session.
        /// </summary>
        [DataMember]
        public string ChallengeId { get; set; }
    }
}