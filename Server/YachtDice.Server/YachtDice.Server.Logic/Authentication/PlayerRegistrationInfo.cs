namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Data needed to register a new player.
    /// </summary>
    public class PlayerRegistrationInfo
    {
        /// <summary>
        /// Player's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Player's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Username chosen by the player.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Player's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Password in plain text; it is hashed before being stored.
        /// </summary>
        public string Password { get; set; }
    }
}