using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;
    using DataExceptions = YachtDice.Server.Data.Exceptions;
    using LogicLogging = YachtDice.Server.Logic.Logging;
    using LogicFriends = YachtDice.Server.Logic.Friends;
    using LogicSecurity = YachtDice.Server.Logic.Security;

    /// <summary>
    /// Common behavior of the services: it runs each operation and converts any failure into a
    /// <see cref="Contracts.ServiceFault"/> that never includes the stack trace. The full error goes to the log.
    /// </summary>
    public abstract class ServiceBase
    {
        private readonly LogicLogging.AppLogger _logger;

        /// <summary>
        /// Initializes the logger with the type of the service that inherits from this class.
        /// </summary>
        protected ServiceBase()
        {
            _logger = new LogicLogging.AppLogger(GetType());
        }

        /// <summary>
        /// Gets or sets the sessions of the players. The host gives the same registry to every service, so a token
        /// that the authentication service created works in all of them.
        /// </summary>
        public LogicSecurity.SessionRegistry Sessions { get; set; } = new LogicSecurity.SessionRegistry();

        /// <summary>
        /// Gets or sets what finds the identifier of the account of a name, to write who did something in the log.
        /// The host gives it to every service; without it the log writes a generic player.
        /// </summary>
        public LogicFriends.IPlayerIdFinder PlayerIds { get; set; } = new UnknownPlayerIds();

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
            RejectMissingData(data, operationName);

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
        /// Runs an operation for the player who owns the session token. A token that is unknown or that ended is
        /// rejected with a fault, so nobody can act in the name of another player.
        /// </summary>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="sessionToken">Token that the client received when the player logged in.</param>
        /// <param name="operation">Operation that receives the name of the player of the session.</param>
        /// <param name="operationName">Name of the operation, for the log.</param>
        /// <returns>The result of the operation.</returns>
        protected TResult ExecuteForPlayer<TResult>(
            string sessionToken,
            Func<string, TResult> operation,
            [CallerMemberName] string operationName = "")
        {
            string playerName = FindSessionPlayer(sessionToken, operationName);

            return Run(() => operation(playerName), operationName);
        }

        /// <summary>
        /// Gets the name of the player who calls an operation that a guest can also use. With a session token,
        /// the name is the one of the session; without it, the player is a guest and keeps the name it sent,
        /// unless the name belongs to an account, so a guest cannot act in the name of a registered player.
        /// </summary>
        /// <param name="sessionToken">Token of the session, or an empty text for a guest.</param>
        /// <param name="guestName">Name that the guest wants to show.</param>
        /// <param name="operationName">Name of the operation, for the log.</param>
        /// <returns>The name of the player who calls.</returns>
        protected string ResolvePlayerName(
            string sessionToken,
            string guestName,
            [CallerMemberName] string operationName = "")
        {
            string playerName = guestName;

            if (!string.IsNullOrEmpty(sessionToken))
            {
                playerName = FindSessionPlayer(sessionToken, operationName);
            }
            else if (PlayerIds.FindPlayerId(guestName) != LogicFriends.FriendshipService.NoPlayerId)
            {
                _logger.Warning(DescribeOperation(LogMessages.AccountNameLogFormat, operationName));

                throw CreateFault(Contracts.ServiceErrorCode.ValidationFailed, LogMessages.AccountNameMessage);
            }

            return playerName;
        }

        /// <summary>
        /// Gets the text that identifies a player in the log: the identifier of the account or a generic text.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The text to write in the log.</returns>
        protected string DescribePlayer(string displayName)
        {
            return PlayerLogName.Describe(PlayerIds, displayName);
        }

        /// <summary>
        /// Writes an informative line in the log of the server.
        /// </summary>
        /// <param name="message">Text to write. It must not include passwords or codes.</param>
        protected void LogInfo(string message)
        {
            _logger.Info(message);
        }

        private string FindSessionPlayer(string sessionToken, string operationName)
        {
            if (!Sessions.TryGetPlayerName(sessionToken, out string playerName))
            {
                throw CreateSessionFault(sessionToken, operationName);
            }

            return playerName;
        }

        private static bool RunOperation(Action operation)
        {
            operation();

            return true;
        }

        private void RejectMissingData(object data, string operationName)
        {
            if (data == null)
            {
                _logger.Warning(DescribeOperation(LogMessages.NullDataLogFormat, operationName));

                throw CreateFault(Contracts.ServiceErrorCode.ValidationFailed, LogMessages.ServiceNullDataMessage);
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

        private sealed class UnknownPlayerIds : LogicFriends.IPlayerIdFinder
        {
            public int FindPlayerId(string displayName)
            {
                return LogicFriends.FriendshipService.NoPlayerId;
            }
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
            _logger.Error(exception, DescribeOperation(LogMessages.DatabaseLogFormat, operationName));

            return CreateFault(Contracts.ServiceErrorCode.DatabaseUnavailable, LogMessages.DatabaseFaultMessage);
        }

        private FaultException<Contracts.ServiceFault> CreateInvalidDataFault(Exception exception, string operationName)
        {
            _logger.Error(exception, DescribeOperation(LogMessages.InvalidDataLogFormat, operationName));

            return CreateFault(Contracts.ServiceErrorCode.ValidationFailed, LogMessages.ServiceInvalidDataMessage);
        }

        private FaultException<Contracts.ServiceFault> CreateUnexpectedFault(Exception exception, string operationName)
        {
            _logger.Error(exception, DescribeOperation(LogMessages.UnexpectedLogFormat, operationName));

            return CreateFault(Contracts.ServiceErrorCode.UnexpectedError, LogMessages.UnexpectedFaultMessage);
        }

        private FaultException<Contracts.ServiceFault> CreateSessionFault(string sessionToken, string operationName)
        {
            _logger.Warning(DescribeOperation(LogMessages.RejectedSessionLogFormat, operationName));

            FaultException<Contracts.ServiceFault> fault =
                CreateFault(Contracts.ServiceErrorCode.SessionExpired, LogMessages.SessionExpiredMessage);

            if (Sessions.WasReplaced(sessionToken))
            {
                fault = CreateFault(Contracts.ServiceErrorCode.SessionReplaced, LogMessages.SessionReplacedMessage);
            }

            return fault;
        }
    }
}