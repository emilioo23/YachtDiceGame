using System;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.TwoFactor;
    using LogicTwoFactor = YachtDice.Server.Logic.TwoFactor;

    /// <summary>
    /// Manages the two-factor authentication configuration operations.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class TwoFactorManagementService : ServiceBase, Contracts.ITwoFactorManagementService
    {
        private readonly LogicTwoFactor.ITwoFactorSettingsService _settingsService;

        /// <summary>
        /// Initializes a new instance of the TwoFactorManagementService class.
        /// </summary>
        /// <param name="settingsService">The service that turns the two-factor verification on or off.</param>
        public TwoFactorManagementService(LogicTwoFactor.ITwoFactorSettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
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
        /// <param name="displayName">Name of the player.</param>
        /// <param name="culture">Culture or language code preferred by the player.</param>
        /// <returns>Outcome of the request and the data of the challenge.</returns>
        public Contracts.TwoFactorActivationData StartTwoFactorActivation(string displayName, string culture)
        {
            Contracts.TwoFactorActivationData activation =
                Execute(displayName, () => ProcessActivationStart(displayName, culture));

            return activation;
        }

        /// <summary>
        /// Checks the code entered by the player and, if correct, turns on the two-step verification.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="verificationCode">Code entered by the player and the challenge of the activation.</param>
        /// <returns>Result of the validation.</returns>
        public Contracts.VerificationResultCode ConfirmTwoFactorActivation(
            string displayName,
            Contracts.VerificationCode verificationCode)
        {
            Contracts.VerificationResultCode result =
                Execute(verificationCode, () => ProcessActivationConfirmation(displayName, verificationCode));

            return result;
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
            Contracts.ResendResultCode result =
                Execute(displayName, () => ProcessActivationResend(displayName, challengeId, culture));

            return result;
        }

        /// <summary>
        /// Turns off the two-step verification after checking the player's password.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request.</returns>
        public Contracts.TwoFactorChangeResultCode DisableTwoFactor(string displayName, string password)
        {
            Contracts.TwoFactorChangeResultCode result =
                Execute(displayName, () => ProcessDisable(displayName, password));

            return result;
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
            string challengeId,
            string culture)
        {
            LogicTwoFactor.ResendCodeStatus logicResult =
                _settingsService.ResendActivationCode(displayName, culture, challengeId);

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