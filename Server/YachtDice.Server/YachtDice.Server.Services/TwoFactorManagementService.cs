using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.TwoFactor;
    using AuthenticationContracts = YachtDice.Server.Contracts.Authentication;
    using LogicAuthentication = YachtDice.Server.Logic.Authentication;
    using LogicPasskeys = YachtDice.Server.Logic.Passkeys;
    using LogicTwoFactor = YachtDice.Server.Logic.TwoFactor;

    /// <summary>
    /// Manages the two-factor authentication configuration operations.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class TwoFactorManagementService : ServiceBase, Contracts.ITwoFactorManagementService
    {
        private readonly LogicTwoFactor.ITwoFactorSettingsService _settingsService;
        private readonly LogicAuthentication.PasswordService _passwordChangeService;
        private readonly LogicPasskeys.IPasskeyService _passkeyService;

        /// <summary>
        /// Initializes a new instance of the TwoFactorManagementService class.
        /// </summary>
        /// <param name="settingsService">The service that turns the two-factor verification on or off.</param>
        /// <param name="passwordChangeService">
        /// The service that changes the password of a player who is logged in.
        /// </param>
        /// <param name="passkeyService">The service that registers the passkeys and logs in with them.</param>
        /// <exception cref="ArgumentNullException">Any of the parameters is null.</exception>
        public TwoFactorManagementService(
            LogicTwoFactor.ITwoFactorSettingsService settingsService,
            LogicAuthentication.PasswordService passwordChangeService,
            LogicPasskeys.IPasskeyService passkeyService)
        {
            _settingsService = settingsService
                ?? throw new ArgumentNullException(nameof(settingsService),
                    LogMessages.TwoFactorManagementNullDependencyMessage);
            _passwordChangeService = passwordChangeService
                ?? throw new ArgumentNullException(nameof(passwordChangeService),
                    LogMessages.TwoFactorManagementNullDependencyMessage);
            _passkeyService = passkeyService
                ?? throw new ArgumentNullException(nameof(passkeyService),
                    LogMessages.TwoFactorManagementNullDependencyMessage);
        }

        /// <summary>
        /// Indicates whether the player has the two-step verification turned on.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>True if the player must enter a code to log in.</returns>
        public bool IsTwoFactorEnabled(string sessionToken)
        {
            bool isEnabled = ExecuteForPlayer(sessionToken, _settingsService.IsEnabled);

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

        /// <summary>
        /// Gives the challenge to create a passkey for the player who is logged in.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The challenge and the data of the player for the authenticator.</returns>
        public Contracts.PasskeyChallengeData BeginPasskeyRegistration(string sessionToken)
        {
            Contracts.PasskeyChallengeData challenge = ExecuteForPlayer(
                sessionToken,
                playerName => ToContract(_passkeyService.BeginRegistration(playerName)));

            return challenge;
        }

        /// <summary>
        /// Checks the answer of the authenticator and saves the new passkey.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="attestation">Answer of the authenticator.</param>
        /// <returns>Outcome of the registration.</returns>
        public Contracts.PasskeyRegistrationResultCode CompletePasskeyRegistration(
            string sessionToken,
            Contracts.PasskeyAttestationData attestation)
        {
            Contracts.PasskeyRegistrationResultCode result = ExecuteForPlayer(
                sessionToken,
                playerName => ProcessPasskeyRegistration(playerName, attestation));

            return result;
        }

        /// <summary>
        /// Counts the passkeys that the player has registered.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The number of passkeys.</returns>
        public int CountPasskeys(string sessionToken)
        {
            int passkeyCount = ExecuteForPlayer(sessionToken, playerName => _passkeyService.CountPasskeys(playerName));

            return passkeyCount;
        }

        /// <summary>
        /// Lists the passkeys that the player has registered.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The passkeys of the player.</returns>
        public List<Contracts.PasskeyInfoData> ListPasskeys(string sessionToken)
        {
            List<Contracts.PasskeyInfoData> passkeys = ExecuteForPlayer(
                sessionToken,
                playerName => ToContract(_passkeyService.ListPasskeys(playerName)));

            return passkeys;
        }

        /// <summary>
        /// Removes a passkey of the player after checking the password.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="passkeyId">Identifier of the passkey to remove.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the removal.</returns>
        public Contracts.PasskeyRemovalResultCode RemovePasskey(string sessionToken, int passkeyId, string password)
        {
            Contracts.PasskeyRemovalResultCode result = ExecuteForPlayer(
                sessionToken,
                playerName => Mapping.EnumMapper.Map<
                    LogicPasskeys.PasskeyRemovalStatus,
                    Contracts.PasskeyRemovalResultCode>(
                    _passkeyService.RemovePasskey(playerName, passkeyId, password)));

            return result;
        }

        /// <summary>
        /// Gives the challenge to log in with a passkey. The player is not known yet, so no session is needed.
        /// </summary>
        /// <returns>The challenge.</returns>
        public Contracts.PasskeyChallengeData BeginPasskeyLogin()
        {
            Contracts.PasskeyChallengeData challenge =
                Execute(() => ToContract(_passkeyService.BeginLogin()));

            return challenge;
        }

        /// <summary>
        /// Checks the answer of the authenticator and, if it is valid, logs the player in.
        /// </summary>
        /// <param name="assertion">Answer of the authenticator.</param>
        /// <returns>Outcome of the login and the data of the new session.</returns>
        public AuthenticationContracts.LoginData CompletePasskeyLogin(Contracts.PasskeyAssertionData assertion)
        {
            AuthenticationContracts.LoginData loginData = Execute(assertion, () => ProcessPasskeyLogin(assertion));

            return loginData;
        }

        private Contracts.PasskeyRegistrationResultCode ProcessPasskeyRegistration(
            string displayName,
            Contracts.PasskeyAttestationData attestation)
        {
            LogicPasskeys.PasskeyRegistrationStatus status =
                _passkeyService.CompleteRegistration(displayName, ToLogic(attestation));

            return Mapping.EnumMapper.Map<
                LogicPasskeys.PasskeyRegistrationStatus,
                Contracts.PasskeyRegistrationResultCode>(status);
        }

        private AuthenticationContracts.LoginData ProcessPasskeyLogin(Contracts.PasskeyAssertionData assertion)
        {
            LogicAuthentication.LoginResult logicResult =
                _passkeyService.CompleteLogin(ToLogic(assertion));
            var loginData = new AuthenticationContracts.LoginData
            {
                Code = Mapping.EnumMapper.Map<
                    LogicAuthentication.LoginStatus,
                    AuthenticationContracts.LoginResultCode>(logicResult.Status),
                DisplayName = logicResult.DisplayName,
                FirstName = logicResult.FirstName,
                ChallengeId = logicResult.ChallengeId,
                SessionToken = string.Empty
            };

            if (logicResult.Status == LogicAuthentication.LoginStatus.LoggedIn)
            {
                loginData.SessionToken = Sessions.Start(logicResult.DisplayName);
            }

            return loginData;
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
            LogicAuthentication.PasswordChangeStatus status =
                _passwordChangeService.ChangePassword(displayName, change?.CurrentPassword, change?.NewPassword);

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

        /// <summary>
        /// Converts the challenge of the logic layer into the contract.
        /// </summary>
        /// <param name="challenge">Challenge of the logic layer.</param>
        /// <returns>The challenge for the client.</returns>
        private static Contracts.PasskeyChallengeData ToContract(LogicPasskeys.PasskeyChallenge challenge)
        {
            return new Contracts.PasskeyChallengeData
            {
                Code = Mapping.EnumMapper.Map<LogicPasskeys.PasskeyStartStatus, Contracts.PasskeyStartResultCode>(
                    challenge.Status),
                ChallengeId = challenge.ChallengeId,
                Challenge = challenge.Challenge,
                UserHandle = challenge.UserHandle,
                UserName = challenge.UserName,
                ExcludedCredentialIds = challenge.ExcludedCredentialIds.ToList()
            };
        }

        /// <summary>
        /// Converts the passkeys of the logic layer into the contract.
        /// </summary>
        /// <param name="summaries">Passkeys of the logic layer.</param>
        /// <returns>The passkeys for the client.</returns>
        private static List<Contracts.PasskeyInfoData> ToContract(IEnumerable<LogicPasskeys.PasskeySummary> summaries)
        {
            return summaries
                .Select(summary => new Contracts.PasskeyInfoData { Id = summary.Id, Name = summary.Name })
                .ToList();
        }

        /// <summary>
        /// Converts the answer of a registration into the type of the logic layer.
        /// </summary>
        /// <param name="attestation">Answer received from the client.</param>
        /// <returns>The answer for the logic layer.</returns>
        /// <exception cref="ArgumentNullException">The answer is null.</exception>
        private static LogicPasskeys.PasskeyAttestation ToLogic(Contracts.PasskeyAttestationData attestation)
        {
            if (attestation == null)
            {
                throw new ArgumentNullException(nameof(attestation), LogMessages.NullAttestationMessage);
            }

            return new LogicPasskeys.PasskeyAttestation
            {
                ChallengeId = attestation.ChallengeId,
                ClientDataJson = attestation.ClientDataJson,
                AttestationObject = attestation.AttestationObject,
                Transport = attestation.Transport
            };
        }

        /// <summary>
        /// Converts the answer of a login into the type of the logic layer.
        /// </summary>
        /// <param name="assertion">Answer received from the client.</param>
        /// <returns>The answer for the logic layer.</returns>
        /// <exception cref="ArgumentNullException">The answer is null.</exception>
        private static LogicPasskeys.PasskeyAssertion ToLogic(Contracts.PasskeyAssertionData assertion)
        {
            if (assertion == null)
            {
                throw new ArgumentNullException(nameof(assertion), LogMessages.NullAttestationMessage);
            }

            return new LogicPasskeys.PasskeyAssertion
            {
                ChallengeId = assertion.ChallengeId,
                CredentialId = assertion.CredentialId,
                ClientDataJson = assertion.ClientDataJson,
                AuthenticatorData = assertion.AuthenticatorData,
                Signature = assertion.Signature,
                UserHandle = assertion.UserHandle
            };
        }
    
    }
}