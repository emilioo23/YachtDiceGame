using System;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.TwoFactor;
    using LogicAuthentication = YachtDice.Server.Logic.Authentication;
    using LogicTwoFactor = YachtDice.Server.Logic.TwoFactor;

    /// <summary>
    /// Manages the two-factor authentication configuration operations.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class TwoFactorManagementService : ServiceBase, Contracts.ITwoFactorManagementService
    {
        private const string NullDependencyMessage = "A dependency of the two-factor management cannot be null.";

        private readonly LogicTwoFactor.ITwoFactorSettingsService _settingsService;
        private readonly LogicAuthentication.PasswordChangeService _passwordChangeService;

        /// <summary>
        /// Initializes a new instance of the TwoFactorManagementService class.
        /// </summary>
        /// <param name="settingsService">The service that turns the two-factor verification on or off.</param>
        /// <param name="passwordChangeService">
        /// The service that changes the password of a player who is logged in.
        /// </param>
        /// <exception cref="ArgumentNullException">Any of the parameters is null.</exception>
        public TwoFactorManagementService(
            LogicTwoFactor.ITwoFactorSettingsService settingsService,
            LogicAuthentication.PasswordChangeService passwordChangeService)
        {
            _settingsService = settingsService
                ?? throw new ArgumentNullException(nameof(settingsService), NullDependencyMessage);
            _passwordChangeService = passwordChangeService
                ?? throw new ArgumentNullException(nameof(passwordChangeService), NullDependencyMessage);
        }

        /// <summary>
        /// Indicates whether the player has the two-step verification turned on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>True if the player must enter a code to log in.</returns>
        public bool IsTwoFactorEnabled(string displayName)
        {
            bool isEnabled = Execute(displayName, () => _settingsService.IsEnabled(displayName));

            return isEnabled;
        }

        /// <summary>
        /// Starts turning on the two-step verification by sending a code to the player's email.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the request and the data of the challenge.</returns>
        public Contracts.TwoFactorActivationData StartTwoFactorActivation(string sessionToken, string culture)
        {
            Contracts.TwoFactorActivationData activation =
                ExecuteForPlayer(sessionToken, playerName => ProcessActivationStart(playerName, culture));

            return activation;
        }

        /// <summary>
        /// Checks the code entered by the player and, if correct, turns on the two-step verification.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="verificationCode">Code entered by the player and the challenge of the activation.</param>
        /// <returns>Result of the validation.</returns>
        public Contracts.VerificationResultCode ConfirmTwoFactorActivation(
            string sessionToken,
            Contracts.VerificationCode verificationCode)
        {
            Contracts.VerificationResultCode result = ExecuteForPlayer(
                sessionToken,
                playerName => ProcessActivationConfirmation(playerName, verificationCode));

            return result;
        }

        /// <summary>
        /// Sends a new code while the player is turning on the two-step verification.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="delivery">Challenge of the activation and culture preferred by the player.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        public Contracts.ResendResultCode ResendActivationCode(string sessionToken, Contracts.CodeDelivery delivery)
        {
            Contracts.ResendResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessActivationResend(playerName, delivery));

            return result;
        }

        /// <summary>
        /// Turns off the two-step verification after checking the player's password.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request.</returns>
        public Contracts.TwoFactorChangeResultCode DisableTwoFactor(string sessionToken, string password)
        {
            Contracts.TwoFactorChangeResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessDisable(playerName, password));

            return result;
        }

        /// <summary>
        /// Changes the password of the player who is logged in.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="change">Current password and new password.</param>
        /// <returns>The result of the change.</returns>
        public Contracts.PasswordChangeResultCode ChangePassword(string sessionToken, Contracts.PasswordChange change)
        {
            Contracts.PasswordChangeResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessPasswordChange(playerName, change));

            return result;
        }

        /// <summary>
        /// Creates new backup codes after checking the player's password. The codes created before stop working.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request and the new codes.</returns>
        public Contracts.BackupCodesData CreateBackupCodes(string sessionToken, string password)
        {
            Contracts.BackupCodesData codesData =
                ExecuteForPlayer(sessionToken, playerName => ProcessBackupCodeCreation(playerName, password));

            return codesData;
        }

        /// <summary>
        /// Counts the backup codes that the player has not used.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The number of backup codes left.</returns>
        public int CountBackupCodes(string sessionToken)
        {
            int codeCount = ExecuteForPlayer(sessionToken, playerName => _settingsService.CountBackupCodes(playerName));

            return codeCount;
        }

        /// <summary>
        /// Gets the last change of password of the player and the last lock of the account, if it was recent.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The summary of the security of the account.</returns>
        public Contracts.SecuritySummary GetSecuritySummary(string sessionToken)
        {
            Contracts.SecuritySummary summary =
                ExecuteForPlayer(sessionToken, playerName => ProcessSecuritySummary(playerName));

            return summary;
        }

        private Contracts.SecuritySummary ProcessSecuritySummary(string displayName)
        {
            LogicAuthentication.SecuritySummary summary = _passwordChangeService.GetSecuritySummary(displayName);

            return new Contracts.SecuritySummary
            {
                HasPasswordChange = summary.HasPasswordChange,
                LastPasswordChange = summary.LastPasswordChange,
                HasRecentLock = summary.HasRecentLock,
                LockedUntil = summary.LockedUntil,
                LockReason = summary.LockReason
            };
        }

        private Contracts.BackupCodesData ProcessBackupCodeCreation(string displayName, string password)
        {
            LogicTwoFactor.BackupCodeCreation creation = _settingsService.CreateBackupCodes(displayName, password);

            var codesData = new Contracts.BackupCodesData
            {
                Code = Mapping.EnumMapper.Map<
                    LogicTwoFactor.BackupCodeCreationStatus,
                    Contracts.BackupCodesResultCode>(creation.Status),
                Codes = creation.Codes
            };

            return codesData;
        }

        private Contracts.PasswordChangeResultCode ProcessPasswordChange(
            string displayName,
            Contracts.PasswordChange change)
        {
            var logicChange = new LogicAuthentication.PasswordChange
            {
                CurrentPassword = change?.CurrentPassword,
                NewPassword = change?.NewPassword
            };
            LogicAuthentication.PasswordChangeStatus status =
                _passwordChangeService.ChangePassword(displayName, logicChange);

            return Mapping.EnumMapper.Map<
                LogicAuthentication.PasswordChangeStatus,
                Contracts.PasswordChangeResultCode>(status);
        }

        private Contracts.TwoFactorActivationData ProcessActivationStart(string displayName, string culture)
        {
            LogicTwoFactor.TwoFactorActivationResult logicResult =
                _settingsService.StartActivation(displayName, Mapping.CultureParser.Parse(culture));

            var activation = new Contracts.TwoFactorActivationData
            {
                Code = Mapping.EnumMapper.Map<
                    LogicTwoFactor.TwoFactorActivationStatus,
                    Contracts.TwoFactorActivationResultCode>(logicResult.Status),
                ChallengeId = logicResult.ChallengeId,
                MaskedEmail = logicResult.MaskedEmail
            };

            return activation;
        }

        private Contracts.VerificationResultCode ProcessActivationConfirmation(
            string displayName,
            Contracts.VerificationCode verificationCode)
        {
            var logicCode = new LogicTwoFactor.VerificationCode
            {
                Code = verificationCode.Code,
                ChallengeId = verificationCode.ChallengeId
            };

            LogicTwoFactor.TwoFactorValidationStatus logicResult =
                _settingsService.ConfirmActivation(displayName, logicCode);

            return Mapping.EnumMapper.Map<LogicTwoFactor.TwoFactorValidationStatus, Contracts.VerificationResultCode>(
                logicResult);
        }

        private Contracts.ResendResultCode ProcessActivationResend(
            string displayName,
            Contracts.CodeDelivery delivery)
        {
            var logicDelivery = new LogicTwoFactor.CodeDelivery
            {
                ChallengeId = delivery.ChallengeId,
                CultureName = delivery.Culture
            };

            LogicTwoFactor.ResendCodeStatus logicResult =
                _settingsService.ResendActivationCode(displayName, logicDelivery);

            return Mapping.EnumMapper.Map<LogicTwoFactor.ResendCodeStatus, Contracts.ResendResultCode>(logicResult);
        }

        private Contracts.TwoFactorChangeResultCode ProcessDisable(string displayName, string password)
        {
            LogicTwoFactor.TwoFactorChangeStatus logicResult = _settingsService.Disable(displayName, password);

            return Mapping.EnumMapper.Map<LogicTwoFactor.TwoFactorChangeStatus, Contracts.TwoFactorChangeResultCode>(
                logicResult);
        }
    }
}