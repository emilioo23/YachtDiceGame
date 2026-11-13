namespace YachtDice.Client.Logic.Authentication
{
    /// <summary>
    /// The challenge that needs a new code and the language of the email, which always travel together when the
    /// player asks for another code.
    /// </summary>
    public sealed class ResendCodeInput
    {
        /// <summary>
        /// Initializes the input.
        /// </summary>
        /// <param name="challengeId">Unique challenge identifier of the session that needs a new code.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        public ResendCodeInput(string challengeId, string culture)
        {
            ChallengeId = challengeId;
            Culture = culture;
        }

        /// <summary>
        /// Gets the unique challenge identifier of the session that needs a new code.
        /// </summary>
        public string ChallengeId { get; }

        /// <summary>
        /// Gets the culture or language code preferred by the player.
        /// </summary>
        public string Culture { get; }
    }
}
