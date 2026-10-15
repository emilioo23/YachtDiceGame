namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Result of the first step of the login.
    /// </summary>
    public class LoginResult
    {
        /// <summary>
        /// Initializes the result.
        /// </summary>
        /// <param name="status">Outcome of the login attempt.</param>
        /// <param name="displayName">Player's name, or an empty text if the login did not succeed.</param>
        public LoginResult(LoginStatus status, string displayName)
        {
            Status = status;
            DisplayName = displayName;
        }

        /// <summary>
        /// Outcome of the login attempt.
        /// </summary>
        public LoginStatus Status { get; }

        /// <summary>
        /// Player's name, used by the client in the verification step.
        /// </summary>
        public string DisplayName { get; }
    }
}