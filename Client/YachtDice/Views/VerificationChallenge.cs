namespace YachtDice.Views
{
    /// <summary>
    /// Data of a verification code that was sent to the player and must be typed in the verification window.
    /// </summary>
    public class VerificationChallenge
    {
        /// <summary>
        /// Gets or sets the email where the code was sent. When the challenge is an activation it is already hidden.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the name of the player.
        /// </summary>
        public string PlayerDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the challenge created by the server.
        /// </summary>
        public string ChallengeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the code confirms the activation of the two-step verification
        /// from the settings instead of a login.
        /// </summary>
        public bool IsActivation { get; set; }
    }
}
