using System;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Authentication;
    using LogicAuth = YachtDice.Server.Logic.Authentication;
    using LogicTwoFactor = YachtDice.Server.Logic.TwoFactor;
    using TwoFactorContracts = YachtDice.Server.Contracts.TwoFactor;

    /// <summary>
    /// Manages the authentication operations for the players.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class AuthenticationService : ServiceBase, Contracts.IAuthenticationService
    {
        private readonly LogicAuth.ILoginService _loginService;
        private readonly LogicAuth.IVerificationService _verificationService;

        /// <summary>
        /// Initializes a new instance of the AuthenticationService class.
        /// </summary>
        /// <param name="loginService">The service responsible for login logic.</param>
        /// <param name="verificationService">The service responsible for two-factor verification.</param>
        public AuthenticationService(
            LogicAuth.ILoginService loginService,
            LogicAuth.IVerificationService verificationService)
        {
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
            _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
        }

        /// <summary>
        /// Checks the credentials and, if correct, sends a verification code.
        /// </summary>
        /// <param name="credentials">Login credentials of the player.</param>
        /// <returns>Outcome and initial session data of the login attempt.</returns>
        public Contracts.LoginData LoginPlayer(Contracts.LoginCredentials credentials)
        {
            Contracts.LoginData loginData = Execute(credentials, () => ProcessLogin(credentials));

            return loginData;
        }

        /// <summary>
        /// Validates the verification code entered by the player.
        /// </summary>
        /// <param name="email">Email address the code was sent to.</param>
        /// <param name="code">Verification code entered by the player.</param>
        /// <param name="challengeId">Unique challenge identifier tied to the login session.</param>
        /// <returns>Result of the validation.</returns>
        public TwoFactorContracts.VerificationResultCode VerifyPlayerCode(string email, string code, string challengeId)
        {
            TwoFactorContracts.VerificationResultCode result =
                Execute(email, () => ProcessCodeVerification(email, code, challengeId));

            return result;
        }

        /// <summary>
        /// Resends a verification code to the player's email address.
        /// </summary>
        /// <param name="email">Email address to send the new code to.</param>
        /// <param name="challengeId">Unique challenge identifier tied to the login session.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        public TwoFactorContracts.ResendResultCode ResendPlayerCode(string email, string challengeId, string culture)
        {
            TwoFactorContracts.ResendResultCode result =
                Execute(email, () => ProcessCodeResend(email, challengeId, culture));

            return result;
        }

        private Contracts.LoginData ProcessLogin(Contracts.LoginCredentials credentials)
        {
            var logicCredentials = new LogicAuth.LoginCredentials
            {
                Email = credentials.Email,
                Password = credentials.Password,
                Culture = Mapping.CultureParser.Parse(credentials.CultureName)
            };

            LogicAuth.LoginResult logicResult = _loginService.LoginPlayer(logicCredentials);

            var loginData = new Contracts.LoginData
            {
                ChallengeId = logicResult.ChallengeId,
                DisplayName = logicResult.DisplayName,
                FirstName = logicResult.FirstName,
                Code = Mapping.EnumMapper.Map<LogicAuth.LoginStatus, Contracts.LoginResultCode>(logicResult.Status)
            };

            return loginData;
        }

        private TwoFactorContracts.VerificationResultCode ProcessCodeVerification(
            string email,
            string code,
            string challengeId)
        {
            LogicTwoFactor.TwoFactorValidationStatus logicResult =
                _verificationService.VerifyPlayerCode(email, code, challengeId);

            return Mapping.EnumMapper.Map<
                LogicTwoFactor.TwoFactorValidationStatus,
                TwoFactorContracts.VerificationResultCode>(
                logicResult);
        }

        private TwoFactorContracts.ResendResultCode ProcessCodeResend(string email, string challengeId, string culture)
        {
            LogicTwoFactor.ResendCodeStatus logicResult =
                _verificationService.ResendPlayerCode(email, culture, challengeId);

            return Mapping.EnumMapper.Map<LogicTwoFactor.ResendCodeStatus, TwoFactorContracts.ResendResultCode>(
                logicResult);
        }
    }
}