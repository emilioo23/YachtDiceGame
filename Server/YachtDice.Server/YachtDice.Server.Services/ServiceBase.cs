using System;
using System.Data;
using System.Data.Common;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Services.Contracts;

namespace YachtDice.Server.Services
{
    /// <summary>
    /// Common behavior of the services: it runs each operation and converts any failure into a
    /// <see cref="ServiceFault"/> that never includes the stack trace. The full error goes to the log.
    /// </summary>
    public abstract class ServiceBase
    {
        private const string NullRequestMessage = "The request cannot be empty.";
        private const string InvalidRequestMessage = "The request has invalid data.";
        private const string DatabaseFaultMessage = "The database is not available.";
        private const string UnexpectedFaultMessage = "An unexpected error occurred.";
        private const string DatabaseLogMessage = "Database error while processing a request.";
        private const string InvalidRequestLogMessage = "A request with invalid data was received.";
        private const string UnexpectedLogMessage = "Unexpected error while processing a request.";

        private readonly AppLogger _logger;

        /// <summary>
        /// Initializes the logger with the type of the service that inherits from this class.
        /// </summary>
        protected ServiceBase()
        {
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Runs an operation and converts any failure into a fault for the client.
        /// </summary>
        /// <typeparam name="TResponse">Type of the answer of the operation.</typeparam>
        /// <param name="request">Request received from the client.</param>
        /// <param name="operation">Operation to run.</param>
        /// <returns>The answer of the operation.</returns>
        protected async Task<TResponse> ExecuteAsync<TResponse>(object request, Func<Task<TResponse>> operation)
        {
            if (request == null)
            {
                throw CreateFault(ServiceErrorCode.InvalidRequest, NullRequestMessage);
            }

            TResponse response;

            try
            {
                response = await operation();
            }
            catch (DbException ex)
            {
                throw CreateDatabaseFault(ex);
            }
            catch (DataException ex)
            {
                throw CreateDatabaseFault(ex);
            }
            catch (ArgumentException ex)
            {
                _logger.Error(ex, InvalidRequestLogMessage);
                throw CreateFault(ServiceErrorCode.InvalidRequest, InvalidRequestMessage);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, UnexpectedLogMessage);
                throw CreateFault(ServiceErrorCode.UnexpectedError, UnexpectedFaultMessage);
            }

            return response;
        }

        /// <summary>
        /// Runs an operation that needs no data from the client and converts any failure into a fault.
        /// </summary>
        /// <typeparam name="TResponse">Type of the answer of the operation.</typeparam>
        /// <param name="operation">Operation to run.</param>
        /// <returns>The answer of the operation.</returns>
        protected Task<TResponse> ExecuteAsync<TResponse>(Func<Task<TResponse>> operation)
        {
            return ExecuteAsync(new object(), operation);
        }

        private static FaultException<ServiceFault> CreateFault(ServiceErrorCode code, string message)
        {
            var fault = new ServiceFault
            {
                Code = code,
                Message = message
            };

            return new FaultException<ServiceFault>(fault, new FaultReason(message));
        }

        private FaultException<ServiceFault> CreateDatabaseFault(Exception exception)
        {
            _logger.Error(exception, DatabaseLogMessage);

            return CreateFault(ServiceErrorCode.DatabaseUnavailable, DatabaseFaultMessage);
        }
    }
}