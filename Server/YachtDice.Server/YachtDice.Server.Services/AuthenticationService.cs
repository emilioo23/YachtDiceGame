using System;
using System.Data;
using System.Data.Common;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.TwoFactor;
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
        private const string NullDependencyMessage = "The dependency cannot be null.";
        private const string InvalidRequestMessage = "The request has invalid data.";
        private const string DatabaseFaultMessage = "The database is not available.";
        private const string UnexpectedFaultMessage = "An unexpected error occurred.";
        private const string DatabaseLogMessage = "Database error while processing a request.";
        private const string InvalidRequestLogMessage = "A request with invalid data was received.";
        private const string UnexpectedLogMessage = "Unexpected error while processing a request.";

        private readonly AppLogger _logger = new AppLogger(typeof(AuthenticationService));
        private readonly LoginService _loginService;
        private readonly RegistrationService _registrationService;
        private readonly TwoFactorVerificationService _verificationService;

        /// <summary>
        /// Initializes the service with the logic it uses.
        /// </summary>
        /// <param name="loginService">Logic that checks the credentials.</param>
        /// <param name="registrationService">Logic that registers the players.</param>
        /// <param name="verificationService">Logic that validates and resends the verification codes.</param>
        public AuthenticationService(
            LoginService loginService,
            RegistrationService registrationService,
            TwoFactorVerificationService verificationService)
        {
            if (loginService == null)
            {
                throw new ArgumentNullException(nameof(loginService), NullDependencyMessage);
            }

            if (registrationService == null)
            {
                throw new ArgumentNullException(nameof(registrationService), NullDependencyMessage);
            }

            if (verificationService == null)
            {
                throw new ArgumentNullException(nameof(verificationService), NullDependencyMessage);
            }

            _loginService = loginService;
            _registrationService = registrationService;
            _verificationService = verificationService;
        }

        /// <summary>
        /// Checks the credentials and, if they are correct, sends the verification code by email.
        /// </summary>
        /// <param name="request">Credentials and language of the player.</param>
        /// <returns>Outcome of the login attempt.</returns>
        public Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            return ExecuteAsync(request, () => ProcessLoginAsync(request));
        }

        /// <summary>
        /// Validates the data and registers a new player.
        /// </summary>
        /// <param name="request">Data of the new player.</param>
        /// <returns>Outcome of the registration.</returns>
        public Task<RegisterResultCode> RegisterAsync(RegisterRequestDto request)
        {
            return ExecuteAsync(request, () => ProcessRegisterAsync(request));
        }

        /// <summary>
        /// Validates the verification code entered by the player.
        /// </summary>
        /// <param name="request">Email and code entered by the player.</param>
        /// <returns>Result of the validation.</returns>
        public Task<VerificationResultCode> VerifyCodeAsync(VerifyCodeRequestDto request)
        {
            return ExecuteAsync(request, () => Task.FromResult(ProcessVerifyCode(request)));
        }

        /// <summary>
        /// Generates a new verification code and sends it by email.
        /// </summary>
        /// <param name="request">Email and language of the player.</param>
        /// <returns>Outcome of the request.</returns>
        public Task<ResendResultCode> ResendCodeAsync(ResendCodeRequestDto request)
        {
            return ExecuteAsync(request, () => ProcessResendCodeAsync(request));
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

        private async Task<TResponse> ExecuteAsync<TResponse>(object request, Func<Task<TResponse>> operation)
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

        private FaultException<ServiceFault> CreateDatabaseFault(Exception exception)
        {
            _logger.Error(exception, DatabaseLogMessage);

            return CreateFault(ServiceErrorCode.DatabaseUnavailable, DatabaseFaultMessage);
        }

        private async Task<LoginResponseDto> ProcessLoginAsync(LoginRequestDto request)
        {
            LoginResult result = await _loginService.LoginAsync(LoginMapper.ToLoginRequest(request));

            return LoginMapper.ToResponse(result);
        }

        private async Task<RegisterResultCode> ProcessRegisterAsync(RegisterRequestDto request)
        {
            RegistrationResult result = await _registrationService.RegisterAsync(
                RegistrationMapper.ToRegisterRequest(request));

            return EnumMapper.Map<RegistrationResult, RegisterResultCode>(result);
        }

        private VerificationResultCode ProcessVerifyCode(VerifyCodeRequestDto request)
        {
            TwoFactorValidationResult result = _verificationService.VerifyCode(request.Email, request.Code);

            return EnumMapper.Map<TwoFactorValidationResult, VerificationResultCode>(result);
        }

        private async Task<ResendResultCode> ProcessResendCodeAsync(ResendCodeRequestDto request)
        {
            ResendCodeStatus status = await _verificationService.ResendCodeAsync(
                request.Email,
                CultureParser.Parse(request.CultureName));

            return EnumMapper.Map<ResendCodeStatus, ResendResultCode>(status);
        }
    }
}