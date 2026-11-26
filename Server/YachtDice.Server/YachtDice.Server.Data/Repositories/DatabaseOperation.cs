using System;
using System.Data.Entity.Core;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using YachtDice.Server.Data.Exceptions;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Runs the operations of the repositories inside a database context and converts the failures of Entity
    /// Framework into <see cref="DataAccessException"/>. The connection string comes from the environment of the
    /// server, so a missing variable is reported as a database failure and not as an unexpected error.
    /// </summary>
    internal static class DatabaseOperation
    {
        private const string PropertySeparator = ", ";
        private const string ServerVariable = "YACHTDICE_DB_SERVER";
        private const string UserVariable = "YACHTDICE_DB_USER";
        private const string PasswordVariable = "YACHTDICE_DB_PASSWORD";
        private const string DatabaseName = "YachtDiceDB";
        private const string ApplicationName = "YachtDice.Server";
        private const string ProviderName = "System.Data.SqlClient";
        private const string ModelMetadata = "res://YachtDice.Server.Data/YachtDiceContext.csdl"
            + "|res://YachtDice.Server.Data/YachtDiceContext.ssdl"
            + "|res://YachtDice.Server.Data/YachtDiceContext.msl";

        /// <summary>
        /// Reads data from the database.
        /// </summary>
        /// <typeparam name="TResult">Type of the data that is read.</typeparam>
        /// <param name="query">Query to run.</param>
        /// <param name="failedMessage">Description of the failure, for the logs.</param>
        /// <returns>The result of the query.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public static TResult Query<TResult>(Func<YachtDiceContext, TResult> query, string failedMessage)
        {
            TResult result;
            string connectionString = BuildConnectionString();

            try
            {
                using (var context = new YachtDiceContext(connectionString))
                {
                    result = query(context);
                }
            }
            catch (EntityException ex)
            {
                throw new DataAccessException(failedMessage, ex);
            }

            return result;
        }

        /// <summary>
        /// Applies a change and saves it in the database.
        /// </summary>
        /// <param name="change">Change to apply.</param>
        /// <param name="failedMessage">Description of the failure, for the logs.</param>
        /// <exception cref="ArgumentException">The data to save does not respect the model.</exception>
        /// <exception cref="DataAccessException">The change could not be saved.</exception>
        public static void Save(Action<YachtDiceContext> change, string failedMessage)
        {
            string connectionString = BuildConnectionString();

            try
            {
                using (var context = new YachtDiceContext(connectionString))
                {
                    change(context);
                    context.SaveChanges();
                }
            }
            catch (DbEntityValidationException ex)
            {
                throw new ArgumentException(DescribeInvalidEntities(ex), ex);
            }
            catch (DbUpdateException ex)
            {
                throw new DataAccessException(failedMessage, ex);
            }
            catch (EntityException ex)
            {
                throw new DataAccessException(failedMessage, ex);
            }
        }

        private static string DescribeInvalidEntities(DbEntityValidationException exception)
        {
            string[] propertyNames = exception.EntityValidationErrors
                .SelectMany(entity => entity.ValidationErrors)
                .Select(error => error.PropertyName)
                .ToArray();

            return string.Format(
                CultureInfo.InvariantCulture,
                LogMessages.InvalidEntityFormat,
                string.Join(PropertySeparator, propertyNames));
        }

        /// <summary>
        /// Builds the connection string with the server, the user and the password of the environment.
        /// </summary>
        /// <returns>The Entity Framework connection string of the database.</returns>
        /// <exception cref="DataAccessException">One of the environment variables is not defined.</exception>
        private static string BuildConnectionString()
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
                string message = string.Format(CultureInfo.InvariantCulture, LogMessages.MissingVariableFormat, name);

                throw new DataAccessException(message);
            }

            return value;
        }
    }
}
