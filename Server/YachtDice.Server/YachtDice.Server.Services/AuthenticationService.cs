using System;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Server.Logic.Authentication;
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
    public class AuthenticationService : ServiceBase, IAuthenticationService
    {
        private const string NullDependencyMessage = "The dependency cannot be null.";

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
        public Task<LoginData> Login(Contracts.LoginCredentials request)
        {
            return ExecuteAsync(request, () => ProcessLogin(request));
        }

        /// <summary>
        /// Validates the data and registers a new player.
        /// </summary>
        /// <param name="request">Data of the new player.</param>
        /// <returns>Outcome of the registration.</returns>
        public Task<RegisterResultCode> Register(Contracts.PlayerRegistrationInfo request)
        {
            return ExecuteAsync(request, () => ProcessRegister(request));
        }

        /// <summary>
        /// Validates the verification code entered by the player.
        /// </summary>
        /// <param name="request">Email and code entered by the player.</param>
        /// <returns>Result of the validation.</returns>
        public Task<VerificationResultCode> VerifyCode(VerificationDetails request)
        {
            return ExecuteAsync(request, () => Task.FromResult(ProcessVerifyCode(request)));
        }

        /// <summary>
        /// Generates a new verification code and sends it by email.
        /// </summary>
        /// <param name="request">Email and language of the player.</param>
        /// <returns>Outcome of the request.</returns>
        public Task<ResendResultCode> ResendCode(ResendCodeDetails request)
        {
            return ExecuteAsync(request, () => ProcessResendCode(request));
        }

        private async Task<LoginData> ProcessLogin(Contracts.LoginCredentials request)
        {
            LoginResult result = await _loginService.LoginAsync(LoginMapper.ToLoginRequest(request));

            return LoginMapper.ToResponse(result);
        }

        private async Task<RegisterResultCode> ProcessRegister(Contracts.PlayerRegistrationInfo request)
        {
            RegistrationResult result = await _registrationService.RegisterAsync(
                RegistrationMapper.ToRegisterRequest(request));

            return EnumMapper.Map<RegistrationResult, RegisterResultCode>(result);
        }

        private VerificationResultCode ProcessVerifyCode(VerificationDetails request)
        {
            TwoFactorValidationResult result = _verificationService.VerifyCode(
                request.Email,
                request.Code,
                request.ChallengeId);

            return EnumMapper.Map<TwoFactorValidationResult, VerificationResultCode>(result);
        }

        private async Task<ResendResultCode> ProcessResendCode(ResendCodeDetails request)
        {
            ResendCodeStatus status = await _verificationService.ResendCodeAsync(
                request.Email,
                CultureParser.Parse(request.CultureName),
                request.ChallengeId);

            return EnumMapper.Map<ResendCodeStatus, ResendResultCode>(status);
        }
    }
}