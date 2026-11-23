using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Lets a player turn the two-step verification on or off and create the backup codes that replace the code
    /// sent by email.
    /// </summary>
    public class TwoFactorSettingsService : ITwoFactorSettingsService
    {
        private const int VisibleLocalPartChars = 2;
        private const int FirstCharacterIndex = 0;
        private const int NoBackupCodes = 0;
        private const char MaskCharacter = '*';
        private const char AtSign = '@';
        private const string NullDeliveryMessage = "The delivery data of the code cannot be null.";
        private const string NullDependencyMessage = "A dependency of the two-factor settings cannot be null.";
        private const string EnabledLogFormat = "Player {0} turned on the two-step verification.";
        private const string DisabledLogFormat = "Player {0} turned off the two-step verification.";
        private const string BackupCodesLogFormat = "Player {0} created new backup codes.";
        private const string WrongPasswordLogFormat = "A wrong password was typed in a security change of player {0}.";

        private readonly AppLogger _logger;
        private readonly IPlayerRepository _playerRepository;
        private readonly TwoFactorVerificationService _verificationService;
        private readonly TwoFactorCodeService _codeService;

        /// <summary>
        /// Initializes the service with the components it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read and update players.</param>
        /// <param name="verificationService">Service that handles verification challenges and codes.</param>
        /// <param name="codeService">Service that creates and sends the verification codes.</param>
        public TwoFactorSettingsService(
            IPlayerRepository playerRepository,
            TwoFactorVerificationService verificationService,
            TwoFactorCodeService codeService)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), NullDependencyMessage);
            _verificationService = verificationService
                ?? throw new ArgumentNullException(nameof(verificationService), NullDependencyMessage);
            _codeService = codeService
                ?? throw new ArgumentNullException(nameof(codeService), NullDependencyMessage);
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Indicates whether the player has the two-step verification turned on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>True if the player must enter a code to log in.</returns>
        public bool IsEnabled(string displayName)
        {
            Player player = FindPlayers(displayName).FirstOrDefault();

            return player != null && _playerRepository.HasTwoFactorEnabled(player.Id);
        }

        /// <summary>
        /// Sends a code to the player's email to confirm that the player wants to turn the verification on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="culture">Language in which the email is written.</param>
        /// <returns>Outcome of the request and the data of the challenge.</returns>
        public TwoFactorActivationResult StartActivation(string displayName, CultureInfo culture)
        {
            Player player = FindPlayers(displayName).FirstOrDefault();
            TwoFactorActivationResult result;

            if (player == null)
            {
                result = CreateFailure(TwoFactorActivationStatus.PlayerNotFound);
            }
            else if (_playerRepository.HasTwoFactorEnabled(player.Id))
            {
                result = CreateFailure(TwoFactorActivationStatus.AlreadyEnabled);
            }
            else if (_verificationService.IsLocked(player.Email))
            {
                result = CreateFailure(TwoFactorActivationStatus.VerificationLocked);
            }
            else
            {
                result = SendActivationCode(player, culture);
            }

            return result;
        }

        /// <summary>
        /// Checks the code entered by the player and, if it is correct, turns the verification on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="verificationCode">Code entered by the player and the activation challenge it answers.</param>
        /// <returns>Result of the validation.</returns>
        public TwoFactorValidationStatus ConfirmActivation(string displayName, VerificationCode verificationCode)
        {
            Player player = FindPlayers(displayName).FirstOrDefault();
            TwoFactorValidationStatus status = TwoFactorValidationStatus.Incorrect;

            if (player != null)
            {
                status = _verificationService.VerifyPlayerCode(player.Email, verificationCode);

                if (status == TwoFactorValidationStatus.Valid)
                {
                    _playerRepository.SaveTwoFactorEnabled(player.Id, true);
                    _logger.Info(Describe(EnabledLogFormat, player));
                }
            }

            return status;
        }

        /// <summary>
        /// Sends a new code while the player is turning the verification on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="delivery">Activation challenge and language in which the email is written.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        /// <exception cref="ArgumentNullException">The delivery data is null.</exception>
        public ResendCodeStatus ResendActivationCode(string displayName, CodeDelivery delivery)
        {
            if (delivery == null)
            {
                throw new ArgumentNullException(nameof(delivery), NullDeliveryMessage);
            }

            Player player = FindPlayers(displayName).FirstOrDefault();
            ResendCodeStatus status = ResendCodeStatus.PlayerNotFound;

            if (player != null)
            {
                status = _verificationService.ResendPlayerCode(player.Email, delivery);
            }

            return status;
        }

        /// <summary>
        /// Turns the verification off after checking the player's password.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request.</returns>
        public TwoFactorChangeStatus Disable(string displayName, string password)
        {
            Player player = FindPlayers(displayName).FirstOrDefault();
            TwoFactorChangeStatus status;

            if (player == null)
            {
                status = TwoFactorChangeStatus.PlayerNotFound;
            }
            else if (!IsPasswordCorrect(player, password))
            {
                _logger.Warning(Describe(WrongPasswordLogFormat, player));
                status = TwoFactorChangeStatus.InvalidPassword;
            }
            else
            {
                _playerRepository.SaveTwoFactorEnabled(player.Id, false);
                _verificationService.RemoveBackupCodes(player.Id);
                _logger.Info(Describe(DisabledLogFormat, player));
                status = TwoFactorChangeStatus.Success;
            }

            return status;
        }

        /// <summary>
        /// Creates new backup codes after checking the player's password. The codes created before stop working.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request and the new codes.</returns>
        public BackupCodeCreation CreateBackupCodes(string displayName, string password)
        {
            Player player = FindPlayers(displayName).FirstOrDefault();
            BackupCodeCreation creation;

            if (player == null)
            {
                creation = new BackupCodeCreation(BackupCodeCreationStatus.PlayerNotFound, new List<string>());
            }
            else if (!IsPasswordCorrect(player, password))
            {
                _logger.Warning(Describe(WrongPasswordLogFormat, player));
                creation = new BackupCodeCreation(BackupCodeCreationStatus.InvalidPassword, new List<string>());
            }
            else if (!_playerRepository.HasTwoFactorEnabled(player.Id))
            {
                creation = new BackupCodeCreation(BackupCodeCreationStatus.TwoFactorDisabled, new List<string>());
            }
            else
            {
                List<string> codes = _verificationService.CreateBackupCodes(player.Id);
                _logger.Info(Describe(BackupCodesLogFormat, player));
                creation = new BackupCodeCreation(BackupCodeCreationStatus.Created, codes);
            }

            return creation;
        }

        /// <summary>
        /// Counts the backup codes that the player has not used.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The number of backup codes left; zero if the player does not exist.</returns>
        public int CountBackupCodes(string displayName)
        {
            Player player = FindPlayers(displayName).FirstOrDefault();
            int codeCount = NoBackupCodes;

            if (player != null)
            {
                codeCount = _verificationService.CountBackupCodes(player.Id);
            }

            return codeCount;
        }

        private static string Describe(string logFormat, Player player)
        {
            return string.Format(CultureInfo.InvariantCulture, logFormat, player.Id);
        }

        private static bool IsPasswordCorrect(Player player, string password)
        {
            return !string.IsNullOrEmpty(password) && CredentialRules.Verify(password, player.PwdHash);
        }

        private static TwoFactorActivationResult CreateFailure(TwoFactorActivationStatus status)
        {
            return new TwoFactorActivationResult(status, string.Empty, string.Empty);
        }

        private static string MaskEmail(string email)
        {
            int atIndex = email.IndexOf(AtSign);
            string maskedEmail = email;

            if (atIndex > VisibleLocalPartChars)
            {
                string visiblePart = email.Substring(FirstCharacterIndex, VisibleLocalPartChars);
                string hiddenPart = new string(MaskCharacter, atIndex - VisibleLocalPartChars);

                maskedEmail = visiblePart + hiddenPart + email.Substring(atIndex);
            }

            return maskedEmail;
        }

        private List<Player> FindPlayers(string displayName)
        {
            var players = new List<Player>();

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                players = _playerRepository.FindByName(displayName);
            }

            return players;
        }

        private TwoFactorActivationResult SendActivationCode(Player player, CultureInfo culture)
        {
            string challengeId = _verificationService.CreateChallenge();
            bool isSent = _codeService.SendCode(player, challengeId, culture);
            TwoFactorActivationResult result = CreateFailure(TwoFactorActivationStatus.CodeSendFailed);

            if (isSent)
            {
                result = new TwoFactorActivationResult(
                    TwoFactorActivationStatus.CodeSent,
                    challengeId,
                    MaskEmail(player.Email));
            }

            return result;
        }
    }
}
