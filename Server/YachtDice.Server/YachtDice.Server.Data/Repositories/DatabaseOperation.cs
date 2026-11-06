using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using YachtDice.Server.Data.Exceptions;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Runs the operations of the repositories inside a database context and converts the failures of Entity
    /// Framework into <see cref="DataAccessException"/>.
    /// </summary>
    internal static class DatabaseOperation
    {
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
        /// <exception cref="DataAccessException">The change could not be saved.</exception>
        public static void Save(Action<YachtDiceContext> change, string failedMessage)
        {
            try
            {
                using (var context = new YachtDiceContext())
                {
                    change(context);
                    context.SaveChanges();
                }
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
    }
}
