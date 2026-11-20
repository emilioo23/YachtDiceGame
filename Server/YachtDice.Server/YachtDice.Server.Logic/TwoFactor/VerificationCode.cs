namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Code typed by the player together with the challenge it answers.
    /// </summary>
    public class VerificationCode
    {
        /// <summary>
        /// Gets or sets the code received by email and typed by the player.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the challenge the code belongs to.
        /// </summary>
        public string ChallengeId { get; set; }
    }
}
