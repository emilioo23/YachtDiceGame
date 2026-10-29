using System;
using System.Data;
using System.Data.Common;
using System.ServiceModel;
using YachtDice.Server.Logic.Logging;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;

    /// <summary>
    /// Common behavior of the services: it runs each operation and converts any failure into a
    /// <see cref="Contracts.ServiceFault"/> that never includes the stack trace. The full error goes to the log.
    /// </summary>
    public abstract class ServiceBase
    {
        private const string NullDataMessage = "The provided data cannot be empty.";
        private const string InvalidDataMessage = "The provided data is invalid.";
        private const string DatabaseFaultMessage = "The database is not available.";
        private const string UnexpectedFaultMessage = "An unexpected error occurred.";
        private const string DatabaseLogMessage = "Database error while processing an operation.";
        private const string InvalidDataLogMessage = "An operation with invalid data was received.";
        private const string UnexpectedLogMessage = "Unexpected error while processing an operation.";

        private readonly AppLogger _logger;

        /// <summary>
        /// Initializes the logger with the type of the service that inherits from this class.
        /// </summary>
        protected ServiceBase()
        {
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Runs a synchronous operation and converts any failure into a fault for the client.
        /// </summary>
        /// <typeparam name="TResponse">Type of the answer of the operation.</typeparam>
        /// <param name="data">Data received from the client.</param>
        /// <param name="operation">Operation to run.</param>
        /// <returns>The answer of the operation.</returns>
        protected TResponse Execute<TResponse>(object data, Func<TResponse> operation)
        {
            if (data == null)
            {
                throw CreateFault(Contracts.ServiceErrorCode.ValidationFailed, NullDataMessage);
            }

            try
            {
                return operation();
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
                _logger.Error(ex, InvalidDataLogMessage);
                throw CreateFault(Contracts.ServiceErrorCode.ValidationFailed, InvalidDataMessage);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, UnexpectedLogMessage);
                throw CreateFault(Contracts.ServiceErrorCode.UnexpectedError, UnexpectedFaultMessage);
            }
        }

        /// <summary>
        /// Runs a synchronous operation that needs no data from the client and converts any failure into a fault.
        /// </summary>
        /// <typeparam name="TResponse">Type of the answer of the operation.</typeparam>
        /// <param name="operation">Operation to run.</param>
        /// <returns>The answer of the operation.</returns>
        protected TResponse Execute<TResponse>(Func<TResponse> operation)
        {
            return Execute(new object(), operation);
        }

        private static FaultException<Contracts.ServiceFault> CreateFault(Contracts.ServiceErrorCode code, string message)
        {
            var fault = new Contracts.ServiceFault
            {
                Code = code,
                Message = message
            };

            return new FaultException<Contracts.ServiceFault>(fault, new FaultReason(message));
        }

        private FaultException<Contracts.ServiceFault> CreateDatabaseFault(Exception exception)
        {
            _logger.Error(exception, DatabaseLogMessage);
            return CreateFault(Contracts.ServiceErrorCode.DatabaseUnavailable, DatabaseFaultMessage);
        }
    }
}