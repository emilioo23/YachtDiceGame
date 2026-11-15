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
        private const string NullDependencyMessage = "A dependency of the authentication service cannot be null.";

        private readonly LogicAuth.ILoginService _loginService;
        private readonly LogicAuth.IVerificationService _verificationService;
        private readonly LogicAuth.IPasswordRecoveryService _passwordRecoveryService;

        /// <summary>
        /// Initializes a new instance of the AuthenticationService class.
        /// </summary>
        /// <param name="loginService">The service responsible for login logic.</param>
        /// <param name="verificationService">The service responsible for two-factor verification.</param>
        /// <param name="passwordRecoveryService">The service responsible for recovering a forgotten password.</param>
        public AuthenticationService(
            LogicAuth.ILoginService loginService,
            LogicAuth.IVerificationService verificationService,
            LogicAuth.IPasswordRecoveryService passwordRecoveryService)
        {
            _loginService = loginService
                ?? throw new ArgumentNullException(nameof(loginService), NullDependencyMessage);
            _verificationService = verificationService
                ?? throw new ArgumentNullException(nameof(verificationService), NullDependencyMessage);
            _passwordRecoveryService = passwordRecoveryService
                ?? throw new ArgumentNullException(nameof(passwordRecoveryService), NullDependencyMessage);
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
        /// <param name="verificationCode">Code entered by the player and the challenge of the login session.</param>
        /// <returns>Result of the validation.</returns>
        public TwoFactorContracts.VerificationResultCode VerifyPlayerCode(
            string email,
            TwoFactorContracts.VerificationCode verificationCode)
        {
            TwoFactorContracts.VerificationResultCode result =
                Execute(verificationCode, () => ProcessCodeVerification(email, verificationCode));

            return result;
        }

        /// <summary>
        /// Resends a verification code to the player's email address.
        /// </summary>
        /// <param name="email">Email address to send the new code to.</param>
        /// <param name="delivery">Challenge of the login session and culture preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        public TwoFactorContracts.ResendResultCode ResendPlayerCode(
            string email,
            TwoFactorContracts.CodeDelivery delivery)
        {
            TwoFactorContracts.ResendResultCode result =
                Execute(delivery, () => ProcessCodeResend(email, delivery));

            return result;
        }

        /// <summary>
        /// Sends a password recovery code to the email of the account.
        /// </summary>
        /// <param name="email">Email of the account to recover.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the request.</returns>
        public Contracts.PasswordResetRequestResultCode RequestPasswordReset(string email, string culture)
        {
            Contracts.PasswordResetRequestResultCode result =
                Execute(email, () => ProcessPasswordResetRequest(email, culture));

            return result;
        }

        /// <summary>
        /// Checks the recovery code and, if it is correct, changes the password of the account.
        /// </summary>
        /// <param name="data">Email, code and new password written by the player.</param>
        /// <returns>Outcome of the confirmation.</returns>
        public Contracts.PasswordResetResultCode ConfirmPasswordReset(Contracts.PasswordResetData data)
        {
            Contracts.PasswordResetResultCode result = Execute(data, () => ProcessPasswordResetConfirmation(data));

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
                Code = Mapping.EnumMapper.Map<LogicAuth.LoginStatus, Contracts.LoginResultCode>(logicResult.Status),
                SessionToken = StartSession(logicResult)
            };

            return loginData;
        }

        private string StartSession(LogicAuth.LoginResult logicResult)
        {
            string sessionToken = string.Empty;

            if (logicResult.Status == LogicAuth.LoginStatus.LoggedIn)
            {
                sessionToken = Sessions.Start(logicResult.DisplayName);
            }
            else if (logicResult.Status == LogicAuth.LoginStatus.VerificationCodeSent)
            {
                sessionToken = Sessions.StartPending(logicResult.DisplayName, logicResult.ChallengeId);
            }

            return sessionToken;
        }

        private TwoFactorContracts.VerificationResultCode ProcessCodeVerification(
            string email,
            TwoFactorContracts.VerificationCode verificationCode)
        {
            var logicCode = new LogicTwoFactor.VerificationCode
            {
                Code = verificationCode.Code,
                ChallengeId = verificationCode.ChallengeId
            };

            LogicTwoFactor.TwoFactorValidationStatus logicResult =
                _verificationService.VerifyPlayerCode(email, logicCode);

            if (logicResult == LogicTwoFactor.TwoFactorValidationStatus.Valid)
            {
                Sessions.Activate(verificationCode.ChallengeId);
            }

            return Mapping.EnumMapper.Map<
                LogicTwoFactor.TwoFactorValidationStatus,
                TwoFactorContracts.VerificationResultCode>(
                logicResult);
        }

        private TwoFactorContracts.ResendResultCode ProcessCodeResend(
            string email,
            TwoFactorContracts.CodeDelivery delivery)
        {
            Sessions.TryGetPendingPlayerName(delivery.ChallengeId, out string ownerName);

            var logicDelivery = new LogicTwoFactor.CodeDelivery
            {
                ChallengeId = delivery.ChallengeId,
                CultureName = delivery.Culture,
                OwnerName = ownerName
            };

            LogicTwoFactor.ResendCodeStatus logicResult =
                _verificationService.ResendPlayerCode(email, logicDelivery);

            return Mapping.EnumMapper.Map<LogicTwoFactor.ResendCodeStatus, TwoFactorContracts.ResendResultCode>(
                logicResult);
        }

        private Contracts.PasswordResetRequestResultCode ProcessPasswordResetRequest(string email, string culture)
        {
            LogicAuth.PasswordResetRequestStatus logicResult =
                _passwordRecoveryService.RequestReset(email, Mapping.CultureParser.Parse(culture));

            return Mapping.EnumMapper.Map<
                LogicAuth.PasswordResetRequestStatus,
                Contracts.PasswordResetRequestResultCode>(
                logicResult);
        }

        private Contracts.PasswordResetResultCode ProcessPasswordResetConfirmation(Contracts.PasswordResetData data)
        {
            var confirmation = new LogicAuth.PasswordResetConfirmation
            {
                Email = data.Email,
                Code = data.Code,
                NewPassword = data.NewPassword
            };

            LogicAuth.PasswordResetStatus logicResult = _passwordRecoveryService.ConfirmReset(confirmation);

            return Mapping.EnumMapper.Map<LogicAuth.PasswordResetStatus, Contracts.PasswordResetResultCode>(
                logicResult);
        }
    }
}