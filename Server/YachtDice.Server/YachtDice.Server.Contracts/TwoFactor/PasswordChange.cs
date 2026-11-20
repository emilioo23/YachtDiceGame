using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// The two passwords of a password change. They always travel together, because the current one proves
    /// that the player is the owner of the account.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class PasswordChange
    {
        /// <summary>
        /// Password that the player has now.
        /// </summary>
        [DataMember]
        public string CurrentPassword { get; set; }

        /// <summary>
        /// Password that the player wants to have.
        /// </summary>
        [DataMember]
        public string NewPassword { get; set; }
    }
}
