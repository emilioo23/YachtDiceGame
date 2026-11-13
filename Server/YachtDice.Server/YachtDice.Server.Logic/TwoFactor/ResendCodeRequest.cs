namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Challenge that needs a new code together with the language of the email that carries it.
    /// </summary>
    public class ResendCodeRequest
    {
        /// <summary>
        /// Gets or sets the identifier of the challenge that needs a new code.
        /// </summary>
        public string ChallengeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the language in which the email is written.
        /// </summary>
        public string CultureName { get; set; }
    }
}
