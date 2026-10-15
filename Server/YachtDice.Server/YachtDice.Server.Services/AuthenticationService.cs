using System;
using System.Data;
using System.Data.Common;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Services.Contracts;
using YachtDice.Server.Services.Mapping;

namespace YachtDice.Server.Services
{
    /// <summary>
    /// Implements the authentication operations on top of the logic layer.
    /// A single instance serves all the clients, so the verification codes kept in memory are shared.
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AuthenticationService : IAuthenticationService
    {
        private const string NullRequestMessage = "The request cannot be empty.";
        private const string NullLoginServiceMessage = "The login service cannot be null.";
        private const string DatabaseFaultMessage = "The database is not available.";
        private const string UnexpectedFaultMessage = "An unexpected error occurred.";
        private const string DatabaseLogMessage = "Database error while processing a login.";
        private const string UnexpectedLogMessage = "Unexpected error while processing a login.";

        private readonly AppLogger _logger = new AppLogger(typeof(AuthenticationService));
        private readonly LoginService _loginService;

        /// <summary>
        /// Initializes the service with the logic it uses.
        /// </summary>
        /// <param name="loginService">Logic that checks the credentials.</param>
        public AuthenticationService(LoginService loginService)
        {
            if (loginService == null)
            {
                throw new ArgumentNullException(nameof(loginService), NullLoginServiceMessage);
            }

            _loginService = loginService;
        }

        /// <summary>
        /// Checks the credentials and, if they are correct, sends the verification code by email.
        /// </summary>
        /// <param name="request">Credentials and language of the player.</param>
        /// <returns>Outcome of the login attempt.</returns>
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            if (request == null)
            {
                throw CreateFault(ServiceErrorCode.InvalidRequest, NullRequestMessage);
            }

            LoginResponseDto response;

            try
            {
                LoginResult result = await _loginService.LoginAsync(LoginMapper.ToLoginRequest(request));
                response = LoginMapper.ToResponse(result);
            }
            catch (DbException ex)
            {
                throw CreateDatabaseFault(ex);
            }
            catch (DataException ex)
            {
                throw CreateDatabaseFault(ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, UnexpectedLogMessage);
                throw CreateFault(ServiceErrorCode.UnexpectedError, UnexpectedFaultMessage);
            }

            return response;
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