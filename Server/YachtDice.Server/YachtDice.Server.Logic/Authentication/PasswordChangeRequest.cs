namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// The two passwords of a password change: the one the player has now and the one the player wants.
    /// </summary>
    public class PasswordChangeRequest
    {
        /// <summary>
        /// Gets or sets the password that the player has now, to prove that the player is the owner.
        /// </summary>
        public string CurrentPassword { get; set; }

        /// <summary>
        /// Gets or sets the password that the player wants to have.
        /// </summary>
        public string NewPassword { get; set; }
    }
}
