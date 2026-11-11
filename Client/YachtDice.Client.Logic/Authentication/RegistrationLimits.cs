namespace YachtDice.Client.Logic.Authentication
{
    /// <summary>
    /// Maximum lengths of the account fields. They are the sizes of the columns in the database, the same ones
    /// the server checks, so the player cannot type more than the server accepts.
    /// </summary>
    public static class RegistrationLimits
    {
        /// <summary>
        /// Maximum number of characters of the username.
        /// </summary>
        public const int UsernameMaxLength = 20;

        /// <summary>
        /// Maximum number of characters of the email.
        /// </summary>
        public const int EmailMaxLength = 100;

        /// <summary>
        /// Maximum number of characters of the first name and of the last name.
        /// </summary>
        public const int NameMaxLength = 50;
    }
}
