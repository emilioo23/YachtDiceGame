using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;
    using DataExceptions = YachtDice.Server.Data.Exceptions;
    using LogicLogging = YachtDice.Server.Logic.Logging;

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
        private const string DatabaseLogFormat = "Database error while processing {0}.";
        private const string InvalidDataLogFormat = "Invalid data was received in {0}.";
        private const string UnexpectedLogFormat = "Unexpected error while processing {0}.";

        private readonly LogicLogging.AppLogger _logger;

        /// <summary>
        /// Initializes the logger with the type of the service that inherits from this class.
        /// </summary>
        protected ServiceBase()
        {
            _logger = new LogicLogging.AppLogger(GetType());
        }

        /// <summary>
        /// Runs a synchronous operation and converts any failure into a fault for the client.
        /// </summary>
        /// <typeparam name="TResult">Type of the answer of the operation.</typeparam>
        /// <param name="data">Data received from the client.</param>
        /// <param name="operation">Operation to run.</param>
        /// <param name="operationName">Name of the service operation, filled by the compiler, for the log.</param>
        /// <returns>The answer of the operation.</returns>
        protected TResult Execute<TResult>(
            object data,
            Func<TResult> operation,
            [CallerMemberName] string operationName = "")
        {
            ValidateData(data);

            return Run(operation, operationName);
        }

        /// <summary>
        /// Runs a synchronous operation that gives no answer and converts any failure into a fault for the client.
        /// </summary>
        /// <param name="data">Data received from the client.</param>
        /// <param name="operation">Operation to run.</param>
        /// <param name="operationName">Name of the service operation, filled by the compiler, for the log.</param>
        protected void Execute(object data, Action operation, [CallerMemberName] string operationName = "")
        {
            Execute(data, () => RunOperation(operation), operationName);
        }

        /// <summary>
        /// Runs a synchronous operation that needs no data from the client and converts any failure into a fault.
        /// </summary>
        /// <typeparam name="TResult">Type of the answer of the operation.</typeparam>
        /// <param name="operation">Operation to run.</param>
        /// <param name="operationName">Name of the service operation, filled by the compiler, for the log.</param>
        /// <returns>The answer of the operation.</returns>
        protected TResult Execute<TResult>(Func<TResult> operation, [CallerMemberName] string operationName = "")
        {
            return Run(operation, operationName);
        }

        /// <summary>
        /// Writes an informative line in the log of the server.
        /// </summary>
        /// <param name="message">Text to write. It must not include passwords or codes.</param>
        protected void LogInfo(string message)
        {
            _logger.Info(message);
        }

        private static bool RunOperation(Action operation)
        {
            operation();

            return true;
        }

        private static void ValidateData(object data)
        {
            if (data == null)
            {
                throw CreateFault(Contracts.ServiceErrorCode.ValidationFailed, NullDataMessage);
            }
        }

        private static FaultException<Contracts.ServiceFault> CreateFault(
            Contracts.ServiceErrorCode code,
            string message)
        {
            var fault = new Contracts.ServiceFault
            {
                Code = code,
                Message = message
            };

            return new FaultException<Contracts.ServiceFault>(fault, new FaultReason(message));
        }

        private TResult Run<TResult>(Func<TResult> operation, string operationName)
        {
            TResult result;

            try
            {
                result = operation();
            }
            catch (FaultException)
            {
                throw;
            }
            catch (DataExceptions.DataAccessException ex)
            {
                throw CreateDatabaseFault(ex, operationName);
            }
            catch (ArgumentException ex)
            {
                throw CreateInvalidDataFault(ex, operationName);
            }
            catch (Exception ex)
            {
                throw CreateUnexpectedFault(ex, operationName);
            }

            return result;
        }

        private static string DescribeOperation(string logFormat, string operationName)
        {
            return string.Format(CultureInfo.InvariantCulture, logFormat, operationName);
        }

        private FaultException<Contracts.ServiceFault> CreateDatabaseFault(Exception exception, string operationName)
        {
            _logger.Error(exception, DescribeOperation(DatabaseLogFormat, operationName));

            return CreateFault(Contracts.ServiceErrorCode.DatabaseUnavailable, DatabaseFaultMessage);
        }

        private FaultException<Contracts.ServiceFault> CreateInvalidDataFault(Exception exception, string operationName)
        {
            _logger.Error(exception, DescribeOperation(InvalidDataLogFormat, operationName));

            return CreateFault(Contracts.ServiceErrorCode.ValidationFailed, InvalidDataMessage);
        }

        private FaultException<Contracts.ServiceFault> CreateUnexpectedFault(Exception exception, string operationName)
        {
            _logger.Error(exception, DescribeOperation(UnexpectedLogFormat, operationName));

            return CreateFault(Contracts.ServiceErrorCode.UnexpectedError, UnexpectedFaultMessage);
        }
    }
}