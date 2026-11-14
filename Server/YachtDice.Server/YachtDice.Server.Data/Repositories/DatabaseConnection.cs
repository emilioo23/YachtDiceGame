using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using YachtDice.Server.Data.Configuration;
using YachtDice.Server.Data.Exceptions;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Builds the connection string of the database from the environment variables of the user.
    /// </summary>
    internal static class DatabaseConnection
    {
        private const string DatabaseName = "YachtDiceDB";
        private const string ApplicationName = "EntityFramework";
        private const string ProviderName = "System.Data.SqlClient";
        private const string ModelMetadata =
            "res://*/YachtDiceContext.csdl|res://*/YachtDiceContext.ssdl|res://*/YachtDiceContext.msl";
        private const string MissingConnectionMessage =
            "The connection of the database is not configured. Set the environment variables " +
            "YACHTDICE_DB_SERVER, YACHTDICE_DB_USER and YACHTDICE_DB_PASSWORD.";

        /// <summary>
        /// Builds the connection string that Entity Framework uses to open the database.
        /// </summary>
        /// <returns>The connection string with the credentials of the environment variables.</returns>
        /// <exception cref="DataAccessException">Any of the database variables is not set.</exception>
        public static string Build()
        {
            string server = EnvironmentSettings.Read(EnvironmentSettings.DatabaseServer);
            string user = EnvironmentSettings.Read(EnvironmentSettings.DatabaseUser);
            string password = EnvironmentSettings.Read(EnvironmentSettings.DatabasePassword);

            if (server.Length == 0 || user.Length == 0 || password.Length == 0)
            {
                throw new DataAccessException(MissingConnectionMessage);
            }

            var providerBuilder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = DatabaseName,
                UserID = user,
                Password = password,
                MultipleActiveResultSets = true,
                Encrypt = true,
                TrustServerCertificate = true,
                ApplicationName = ApplicationName
            };

            var entityBuilder = new EntityConnectionStringBuilder
            {
                Metadata = ModelMetadata,
                Provider = ProviderName,
                ProviderConnectionString = providerBuilder.ConnectionString
            };

            return entityBuilder.ConnectionString;
        }
    }
}
