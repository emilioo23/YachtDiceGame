using System;

namespace YachtDice.Server.Data.Configuration
{
    /// <summary>
    /// Reads the secrets of the server (database and mail credentials) from the environment variables of the user,
    /// so they are never written in the source code or in the repository.
    /// </summary>
    public static class EnvironmentSettings
    {
        /// <summary>
        /// Name of the variable with the address of the SQL Server instance.
        /// </summary>
        public const string DatabaseServer = "YACHTDICE_DB_SERVER";

        /// <summary>
        /// Name of the variable with the user of the database.
        /// </summary>
        public const string DatabaseUser = "YACHTDICE_DB_USER";

        /// <summary>
        /// Name of the variable with the password of the user of the database.
        /// </summary>
        public const string DatabasePassword = "YACHTDICE_DB_PASSWORD";

        /// <summary>
        /// Name of the variable with the account that sends the emails.
        /// </summary>
        public const string SmtpUser = "YACHTDICE_SMTP_USER";

        /// <summary>
        /// Name of the variable with the password of the account that sends the emails.
        /// </summary>
        public const string SmtpPassword = "YACHTDICE_SMTP_PASSWORD";

        /// <summary>
        /// Reads the value of an environment variable.
        /// </summary>
        /// <param name="name">Name of the environment variable.</param>
        /// <returns>The value of the variable, or an empty text when it is not set.</returns>
        public static string Read(string name)
        {
            string value = Environment.GetEnvironmentVariable(name);

            return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }
    }
}
