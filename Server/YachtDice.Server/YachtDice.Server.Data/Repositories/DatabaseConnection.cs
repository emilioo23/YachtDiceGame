using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Globalization;
using YachtDice.Server.Data.Exceptions;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Builds the connection string of the database from the environment variables of the computer that runs
    /// the server. The user and the password of the database are never written in a file of the project, and a
    /// container receives them the same way as a computer.
    /// </summary>
    internal static class DatabaseConnection
    {
        private const string ServerVariable = "YACHTDICE_DB_SERVER";
        private const string UserVariable = "YACHTDICE_DB_USER";
        private const string PasswordVariable = "YACHTDICE_DB_PASSWORD";
        private const string DatabaseName = "YachtDiceDB";
        private const string ApplicationName = "YachtDice.Server";
        private const string ProviderName = "System.Data.SqlClient";
        private const string ModelMetadata = "res://YachtDice.Server.Data/YachtDiceContext.csdl"
            + "|res://YachtDice.Server.Data/YachtDiceContext.ssdl"
            + "|res://YachtDice.Server.Data/YachtDiceContext.msl";
        private const string MissingVariableFormat = "The environment variable {0} is not defined on this computer.";

        /// <summary>
        /// Builds the connection string with the server, the user and the password of the environment.
        /// </summary>
        /// <returns>The Entity Framework connection string of the database.</returns>
        /// <exception cref="DataAccessException">One of the environment variables is not defined.</exception>
        public static string Build()
        {
            var databaseConnection = new SqlConnectionStringBuilder
            {
                DataSource = ReadVariable(ServerVariable),
                InitialCatalog = DatabaseName,
                UserID = ReadVariable(UserVariable),
                Password = ReadVariable(PasswordVariable),
                MultipleActiveResultSets = true,
                Encrypt = true,
                TrustServerCertificate = true,
                ApplicationName = ApplicationName
            };
            var entityConnection = new EntityConnectionStringBuilder
            {
                Metadata = ModelMetadata,
                Provider = ProviderName,
                ProviderConnectionString = databaseConnection.ConnectionString
            };

            return entityConnection.ConnectionString;
        }

        private static string ReadVariable(string name)
        {
            string value = Environment.GetEnvironmentVariable(name);

            if (string.IsNullOrWhiteSpace(value))
            {
                string message = string.Format(CultureInfo.InvariantCulture, MissingVariableFormat, name);

                throw new DataAccessException(message);
            }

            return value;
        }
    }
}
