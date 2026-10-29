using System;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;
    using LogicAuth = YachtDice.Server.Logic.Authentication;
    using LogicTwoFactor = YachtDice.Server.Logic.TwoFactor;

    /// <summary>
    /// Manages the authentication operations for the players.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AuthenticationService : ServiceBase, Contracts.IAuthenticationService
    {
        private readonly LogicAuth.ILoginService _loginService;
        private readonly LogicAuth.IVerificationService _verificationService;

        /// <summary>
        /// Initializes a new instance of the AuthenticationService class.
        /// </summary>
        /// <param name="loginService">The service responsible for login logic.</param>
        /// <param name="verificationService">The service responsible for two-factor verification.</param>
        public AuthenticationService(LogicAuth.ILoginService loginService, LogicAuth.IVerificationService verificationService)
        {
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
            _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
        }

        /// <summary>
        /// Checks the credentials and, if correct, sends a verification code.
        /// </summary>
        /// <param name="credentials">Login credentials of the player.</param>
        /// <returns>Outcome and initial session data of the login attempt.</returns>
        public Contracts.LoginData Login(Contracts.LoginCredentials credentials)
        {
            return Execute(credentials, () => ProcessLogin(credentials));
        }

        /// <summary>
        /// Validates the verification code entered by the player.
        /// </summary>
        /// <param name="email">Email address the code was sent to.</param>
        /// <param name="code">Verification code entered by the player.</param>
        /// <param name="challengeId">Unique challenge identifier tied to the login session.</param>
        /// <returns>Result of the validation.</returns>
        public Contracts.VerificationResultCode VerifyCode(string email, string code, string challengeId)
        {
            return Execute(email, () => ProcessVerifyCode(email, code, challengeId));
        }

        /// <summary>
        /// Resends a verification code to the player's email address.
        /// </summary>
        /// <param name="email">Email address to send the new code to.</param>
        /// <param name="challengeId">Unique challenge identifier tied to the login session.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        public Contracts.ResendResultCode ResendCode(string email, string challengeId, string culture)
        {
            return Execute(email, () => ProcessResendCode(email, challengeId, culture));
        }

        /// <summary>
        /// Processes the login logic synchronously by mapping contracts to domain models.
        /// </summary>
        /// <param name="credentials">The login credentials from the client.</param>
        /// <returns>The mapped login data result.</returns>
        private Contracts.LoginData ProcessLogin(Contracts.LoginCredentials credentials)
        {
            var logicCredentials = new LogicAuth.LoginCredentials
            {
                Email = credentials.Email,
                Password = credentials.Password,
                Culture = Mapping.CultureParser.Parse(credentials.CultureName)
            };

            LogicAuth.LoginResult logicResult = _loginService.Login(logicCredentials);

            return new Contracts.LoginData
            {
                ChallengeId = logicResult.ChallengeId.ToString(),
                DisplayName = logicResult.DisplayName,
                Code = (Contracts.LoginResultCode)(int)logicResult.Status
            };
        }

        /// <summary>
        /// Processes the verification code logic synchronously.
        /// </summary>
        /// <param name="email">The player's email.</param>
        /// <param name="code">The verification code.</param>
        /// <param name="challengeId">The challenge identifier.</param>
        /// <returns>The verification result code.</returns>
        private Contracts.VerificationResultCode ProcessVerifyCode(string email, string code, string challengeId)
        {
            LogicTwoFactor.TwoFactorValidationStatus logicResult = _verificationService.VerifyCode(email, code, challengeId);

            return (Contracts.VerificationResultCode)(int)logicResult;
        }

        /// <summary>
        /// Processes the resend code logic synchronously.
        /// </summary>
        /// <param name="email">The player's email.</param>
        /// <param name="challengeId">The challenge identifier.</param>
        /// <param name="culture">The culture name.</param>
        /// <returns>The resend result code.</returns>
        private Contracts.ResendResultCode ProcessResendCode(string email, string challengeId, string culture)
        {
            LogicTwoFactor.ResendCodeStatus logicResult = _verificationService.ResendCode(email, culture, challengeId);

            return (Contracts.ResendResultCode)(int)logicResult;
        }
    }
}