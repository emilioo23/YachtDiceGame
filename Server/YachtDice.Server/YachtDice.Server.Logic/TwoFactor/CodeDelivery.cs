namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Challenge that needs a new code together with the language of the email that carries it.
    /// </summary>
    public class CodeDelivery
    {
        /// <summary>
        /// Gets or sets the identifier of the challenge that needs a new code.
        /// </summary>
        public string ChallengeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the language in which the email is written.
        /// </summary>
        public string CultureName { get; set; }

        /// <summary>
        /// Gets or sets the name of the player that the server issued the challenge to when the player logged
        /// in. It is empty when the challenge is not one that the server issued, so a code is never sent
        /// because somebody wrote an email.
        /// </summary>
        public string OwnerName { get; set; }
    }
}
