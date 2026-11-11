using System;
using System.Configuration;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using YachtDice.Server.Data.Exceptions;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Runs the operations of the repositories inside a database context and converts the failures of Entity
    /// Framework into <see cref="DataAccessException"/>. It also checks that the connection string exists before
    /// opening the context, so a missing configuration file is reported as a database failure and not as an
    /// unexpected error.
    /// </summary>
    internal static class DatabaseOperation
    {
        private const string ConnectionName = "YachtDiceContext";
        private const string PropertySeparator = ", ";
        private const string MissingConnectionMessage = "The connection string of the database is not configured.";
        private const string ConfigurationFailedMessage = "The configuration of the database could not be read.";
        private const string InvalidEntityFormat = "The data did not pass the model validation. Properties: {0}.";

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

            EnsureConnectionIsConfigured();

            try
            {
                using (var context = new YachtDiceContext())
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
            EnsureConnectionIsConfigured();

            try
            {
                using (var context = new YachtDiceContext())
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

        private static void EnsureConnectionIsConfigured()
        {
            ConnectionStringSettings settings;

            try
            {
                settings = ConfigurationManager.ConnectionStrings[ConnectionName];
            }
            catch (ConfigurationErrorsException ex)
            {
                throw new DataAccessException(ConfigurationFailedMessage, ex);
            }

            if (settings == null)
            {
                throw new DataAccessException(MissingConnectionMessage);
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
                InvalidEntityFormat,
                string.Join(PropertySeparator, propertyNames));
        }
    }
}
