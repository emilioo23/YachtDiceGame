using System;
using System.Globalization;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Lets a player recover the password of the account with a code sent by email. Only the hash of the code is
    /// saved, the code expires and it is discarded after too many wrong attempts.
    /// </summary>
    public class PasswordRecoveryService : IPasswordRecoveryService
    {
        /// <summary>
        /// Number of digits of the recovery code.
        /// </summary>
        public const int CodeLength = 6;

        /// <summary>
        /// Minutes a recovery code stays valid.
        /// </summary>
        public const int ExpirationMinutes = 10;

        private const int MaxFailedAttempts = 3;
        private const int ResendCooldownSeconds = 30;
        private const string ChangedLogMessage = "Password changed with a recovery code.";
        private const string TooManyAttemptsLogMessage = "Recovery code discarded after too many wrong attempts.";
        private const string NullArgumentMessage = "The value cannot be null.";
        private const string EmailSubjectResourceKey = "PasswordReset_EmailSubject";
        private const string EmailBodyResourceKey = "PasswordReset_EmailBody";

        private static readonly CodeEmailTemplate EmailTemplate =
            new CodeEmailTemplate(EmailSubjectResourceKey, EmailBodyResourceKey, ExpirationMinutes);

        private readonly AppLogger _logger;
        private readonly IPlayerRepository _playerRepository;
        private readonly IPasswordResetRepository _resetRepository;
        private readonly CodeEmailSender _emailSender;
        private readonly PasswordResetAttemptCounter _attempts = new PasswordResetAttemptCounter();

        /// <summary>
        /// Initializes the service with the components it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read players.</param>
        /// <param name="resetRepository">Repository that keeps the recovery codes.</param>
        /// <param name="emailService">Service that delivers the email with the recovery code.</param>
        public PasswordRecoveryService(
            IPlayerRepository playerRepository,
            IPasswordResetRepository resetRepository,
            IEmailService emailService)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository));
            _resetRepository = resetRepository
                ?? throw new ArgumentNullException(nameof(resetRepository));
            _emailSender = new CodeEmailSender(emailService, EmailTemplate);
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Generates a recovery code and sends it to the email of the account.
        /// </summary>
        /// <param name="email">Email of the account to recover.</param>
        /// <param name="culture">Language in which the email is written.</param>
        /// <returns>Outcome of the request.</returns>
        public PasswordResetRequestStatus RequestReset(string email, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture), NullArgumentMessage);
            }

            PasswordResetRequestStatus status = PasswordResetRequestStatus.MissingRequiredField;

            if (!string.IsNullOrWhiteSpace(email))
            {
                Player player = _playerRepository.FindByEmail(email.Trim()).FirstOrDefault();
                status = PasswordResetRequestStatus.CodeSent;

                if (player != null && !WasCodeSentRecently(player.Id))
                {
                    status = CreateAndSendCode(player, culture);
                }
            }

            return status;
        }

        /// <summary>
        /// Checks the recovery code and, if it is correct, saves the new password.
        /// </summary>
        /// <param name="confirmation">Email, code and new password written by the player.</param>
        /// <returns>Outcome of the confirmation.</returns>
        public PasswordResetStatus ConfirmReset(PasswordResetConfirmation confirmation)
        {
            if (confirmation == null)
            {
                throw new ArgumentNullException(nameof(confirmation), NullArgumentMessage);
            }

            PasswordResetStatus status;

            if (!HasAllFields(confirmation))
            {
                status = PasswordResetStatus.MissingRequiredField;
            }
            else if (!CredentialRules.IsStrongPassword(confirmation.NewPassword))
            {
                status = PasswordResetStatus.WeakPassword;
            }
            else
            {
                status = CheckPlayerCode(confirmation);
            }

            return status;
        }

        private static bool HasAllFields(PasswordResetConfirmation confirmation)
        {
            return !string.IsNullOrWhiteSpace(confirmation.Email)
                && !string.IsNullOrWhiteSpace(confirmation.Code)
                && !string.IsNullOrWhiteSpace(confirmation.NewPassword);
        }

        private bool WasCodeSentRecently(int playerId)
        {
            PwdResetToken token = _resetRepository.FindByPlayerId(playerId).FirstOrDefault();
            var isRecent = false;

            if (token != null)
            {
                DateTime createdAt = token.ExpireDate.AddMinutes(-ExpirationMinutes);
                isRecent = DateTime.UtcNow < createdAt.AddSeconds(ResendCooldownSeconds);
            }

            return isRecent;
        }

        private PasswordResetRequestStatus CreateAndSendCode(Player player, CultureInfo culture)
        {
            string code = NumericCodeGenerator.Generate(CodeLength);
            string codeHash = PasswordResetCodeHasher.Compute(player.Id, code);

            _resetRepository.Save(player.Id, codeHash, DateTime.UtcNow.AddMinutes(ExpirationMinutes));
            _attempts.Reset(player.Id);

            bool isSent = _emailSender.Send(player, code, culture);

            if (!isSent)
            {
                _resetRepository.Delete(player.Id);
            }

            return isSent ? PasswordResetRequestStatus.CodeSent : PasswordResetRequestStatus.CodeSendFailed;
        }

        private PasswordResetStatus CheckPlayerCode(PasswordResetConfirmation confirmation)
        {
            Player player = _playerRepository.FindByEmail(confirmation.Email.Trim()).FirstOrDefault();
            PasswordResetStatus status = PasswordResetStatus.CodeExpired;

            if (player != null)
            {
                status = CheckSavedCode(player, confirmation);
            }

            return status;
        }

        private PasswordResetStatus CheckSavedCode(Player player, PasswordResetConfirmation confirmation)
        {
            PwdResetToken token = _resetRepository.FindByPlayerId(player.Id).FirstOrDefault();
            PasswordResetStatus status = PasswordResetStatus.CodeExpired;

            if (token != null && token.ExpireDate < DateTime.UtcNow)
            {
                _resetRepository.Delete(player.Id);
            }
            else if (token != null)
            {
                status = CheckCodeMatch(player.Id, token, confirmation);
            }

            return status;
        }

        private PasswordResetStatus CheckCodeMatch(
            int playerId,
            PwdResetToken token,
            PasswordResetConfirmation confirmation)
        {
            string enteredHash = PasswordResetCodeHasher.Compute(playerId, confirmation.Code.Trim());
            bool isCorrect = string.Equals(enteredHash, token.Token, StringComparison.OrdinalIgnoreCase);
            PasswordResetStatus status;

            if (isCorrect)
            {
                status = ChangePassword(playerId, confirmation.NewPassword);
            }
            else
            {
                status = RegisterWrongCode(playerId);
            }

            return status;
        }

        private PasswordResetStatus ChangePassword(int playerId, string newPassword)
        {
            _resetRepository.CompleteReset(playerId, PasswordHasher.ComputeHash(newPassword));
            _attempts.Reset(playerId);
            _logger.Info(ChangedLogMessage);

            return PasswordResetStatus.PasswordChanged;
        }

        private PasswordResetStatus RegisterWrongCode(int playerId)
        {
            PasswordResetStatus status = PasswordResetStatus.IncorrectCode;

            if (_attempts.RegisterFailure(playerId) >= MaxFailedAttempts)
            {
                _resetRepository.Delete(playerId);
                _attempts.Reset(playerId);
                _logger.Info(TooManyAttemptsLogMessage);
                status = PasswordResetStatus.TooManyAttempts;
            }

            return status;
        }
    }
}
