namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Defines the business logic operations for player authentication.
    /// </summary>
    public interface ILoginService
    {
        /// <summary>
        /// Validates the provided credentials.
        /// </summary>
        /// <param name="credentials">The player's login credentials.</param>
        /// <returns>The result of the authentication attempt.</returns>
        LoginResult Login(LoginCredentials credentials);
    }
}