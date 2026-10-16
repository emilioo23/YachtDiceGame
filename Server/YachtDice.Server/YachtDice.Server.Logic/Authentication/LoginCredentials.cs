using System.Globalization;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Data needed to start a login.
    /// </summary>
    public class LoginCredentials
    {
        /// <summary>
        /// Player's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Password in plain text; it is compared against the stored hash.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Language in which the verification email is written.
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;
    }
}