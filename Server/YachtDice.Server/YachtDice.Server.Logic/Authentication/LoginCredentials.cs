using System.Globalization;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Data needed to start a login.
    /// </summary>
    public class LoginCredentials
    {
        /// <summary>
        /// Email address or username of the player.
        /// </summary>
        public string Identifier { get; set; }

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