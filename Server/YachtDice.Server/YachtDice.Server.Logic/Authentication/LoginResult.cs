namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Represents the outcome of an authentication attempt.
    /// </summary>
    public class LoginResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginResult"/> class.
        /// </summary>
        /// <param name="status">Status of the login.</param>
        /// <param name="displayName">Player's display name.</param>
        public LoginResult(LoginStatus status, string displayName)
            : this(status, displayName, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginResult"/> class with its verification challenge.
        /// </summary>
        /// <param name="status">Status of the login.</param>
        /// <param name="displayName">Player's display name.</param>
        /// <param name="challengeId">Identifier of the verification challenge the code was sent for.</param>
        public LoginResult(LoginStatus status, string displayName, string challengeId)
        {
            Status = status;
            DisplayName = displayName;
            ChallengeId = challengeId;
        }

        /// <summary>
        /// Status of the login attempt.
        /// </summary>
        public LoginStatus Status { get; }

        /// <summary>
        /// Player's display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Identifier of the verification challenge; empty if no code was sent.
        /// </summary>
        public string ChallengeId { get; }
    }
}