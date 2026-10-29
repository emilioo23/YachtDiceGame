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
        private readonly LogicTwoFactor.ITwoFactorSettingsService _settingsService;

        /// <summary>
        /// Initializes a new instance of the AuthenticationService class.
        /// </summary>
        /// <param name="loginService">The service responsible for login logic.</param>
        /// <param name="verificationService">The service responsible for two-factor verification.</param>
        /// <param name="settingsService">The service that turns the two-factor verification on or off.</param>
        public AuthenticationService(
            LogicAuth.ILoginService loginService,
            LogicAuth.IVerificationService verificationService,
            LogicTwoFactor.ITwoFactorSettingsService settingsService)
        {
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
            _verificationService = verificationService ?? throw new ArgumentNullException(nameof(verificationService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        /// <summary>
        /// Checks the credentials and, if correct, sends a verification code.
        /// </summary>
        /// <param name="credentials">Login credentials of the player.</param>
        /// <returns>Outcome and initial session data of the login attempt.</returns>
        public Contracts.LoginData LoginPlayer(Contracts.LoginCredentials credentials)
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
        public Contracts.VerificationResultCode VerifyPlayerCode(string email, string code, string challengeId)
        {
            return Execute(email, () => ProcessCodeVerification(email, code, challengeId));
        }

        /// <summary>
        /// Resends a verification code to the player's email address.
        /// </summary>
        /// <param name="email">Email address to send the new code to.</param>
        /// <param name="challengeId">Unique challenge identifier tied to the login session.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        public Contracts.ResendResultCode ResendPlayerCode(string email, string challengeId, string culture)
        {
            return Execute(email, () => ProcessCodeResend(email, challengeId, culture));
        }

        /// <summary>
        /// Indicates whether the player has the two-step verification turned on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>True if the player must enter a code to log in.</returns>
        public bool IsTwoFactorEnabled(string displayName)
        {
            return Execute(displayName, () => _settingsService.IsEnabled(displayName));
        }

        /// <summary>
        /// Starts turning on the two-step verification by sending a code to the player's email.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the request and the data of the challenge.</returns>
        public Contracts.TwoFactorActivationData StartTwoFactorActivation(string displayName, string culture)
        {
            return Execute(displayName, () => ProcessActivationStart(displayName, culture));
        }

        /// <summary>
        /// Checks the code entered by the player and, if correct, turns on the two-step verification.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="code">Verification code entered by the player.</param>
        /// <param name="challengeId">Unique challenge identifier of the activation.</param>
        /// <returns>Result of the validation.</returns>
        public Contracts.VerificationResultCode ConfirmTwoFactorActivation(
            string displayName,
            string code,
            string challengeId)
        {
            return Execute(displayName, () => ProcessActivationConfirmation(displayName, code, challengeId));
        }

        /// <summary>
        /// Sends a new code while the player is turning on the two-step verification.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="challengeId">Unique challenge identifier of the activation.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        public Contracts.ResendResultCode ResendActivationCode(string displayName, string challengeId, string culture)
        {
            return Execute(displayName, () => ProcessActivationResend(displayName, challengeId, culture));
        }

        /// <summary>
        /// Turns off the two-step verification after checking the player's password.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request.</returns>
        public Contracts.TwoFactorChangeResultCode DisableTwoFactor(string displayName, string password)
        {
            return Execute(displayName, () => ProcessDisable(displayName, password));
        }

        private Contracts.TwoFactorActivationData ProcessActivationStart(string displayName, string culture)
        {
            LogicTwoFactor.TwoFactorActivationResult logicResult =
                _settingsService.StartActivation(displayName, Mapping.CultureParser.Parse(culture));

            return new Contracts.TwoFactorActivationData
            {
                Code = (Contracts.TwoFactorActivationResultCode)(int)logicResult.Status,
                ChallengeId = logicResult.ChallengeId,
                MaskedEmail = logicResult.MaskedEmail
            };
        }

        private Contracts.VerificationResultCode ProcessActivationConfirmation(
            string displayName,
            string code,
            string challengeId)
        {
            LogicTwoFactor.TwoFactorValidationStatus logicResult =
                _settingsService.ConfirmActivation(displayName, code, challengeId);

            return (Contracts.VerificationResultCode)(int)logicResult;
        }

        private Contracts.ResendResultCode ProcessActivationResend(
            string displayName,
            string challengeId,
            string culture)
        {
            LogicTwoFactor.ResendCodeStatus logicResult =
                _settingsService.ResendActivationCode(displayName, culture, challengeId);

            return (Contracts.ResendResultCode)(int)logicResult;
        }

        private Contracts.TwoFactorChangeResultCode ProcessDisable(string displayName, string password)
        {
            LogicTwoFactor.TwoFactorChangeStatus logicResult = _settingsService.Disable(displayName, password);

            return (Contracts.TwoFactorChangeResultCode)(int)logicResult;
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

            return new Contracts.LoginData
            {
                ChallengeId = logicResult.ChallengeId.ToString(),
                DisplayName = logicResult.DisplayName,
                Code = (Contracts.LoginResultCode)(int)logicResult.Status
            };
        }

        private Contracts.VerificationResultCode ProcessCodeVerification(
            string email,
            string code,
            string challengeId)
        {
            LogicTwoFactor.TwoFactorValidationStatus logicResult =
                _verificationService.VerifyPlayerCode(email, code, challengeId);

            return (Contracts.VerificationResultCode)(int)logicResult;
        }

        private Contracts.ResendResultCode ProcessCodeResend(string email, string challengeId, string culture)
        {
            LogicTwoFactor.ResendCodeStatus logicResult =
                _verificationService.ResendPlayerCode(email, culture, challengeId);

            return (Contracts.ResendResultCode)(int)logicResult;
        }
    }
}