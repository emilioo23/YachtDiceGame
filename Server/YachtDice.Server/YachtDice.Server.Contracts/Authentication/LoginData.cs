using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Authentication
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
        /// Email of the account, so the client can verify the code of the login even when the player typed the
        /// username; empty if the password was not correct.
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// First name of the player, used only to greet them.
        /// </summary>
        [DataMember]
        public string FirstName { get; set; }

        /// <summary>
        /// Unique challenge identifier tied to the login session.
        /// </summary>
        [DataMember]
        public string ChallengeId { get; set; }

        /// <summary>
        /// Secret token of the session. The client sends it instead of the name in the operations that change the
        /// data of the player. When a code is needed, the token works only after the code is confirmed.
        /// </summary>
        [DataMember]
        public string SessionToken { get; set; }
    }
}
