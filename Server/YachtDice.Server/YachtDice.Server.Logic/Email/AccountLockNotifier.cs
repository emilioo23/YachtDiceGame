using System;
using System.Globalization;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Resources;

namespace YachtDice.Server.Logic.Email
{
    /// <summary>
    /// Tells a player by email that the account was locked for the day because of too many wrong passwords,
    /// so the owner finds out if somebody else is trying to enter the account.
    /// </summary>
    public class AccountLockNotifier
    {
        /// <summary>
        /// Hours that the account stays locked after too many wrong passwords in one day.
        /// </summary>
        public const int LockHours = 24;

        private const string SubjectResourceKey = "AccountLock_EmailSubject";
        private const string BodyResourceKey = "AccountLock_EmailBody";
        private const string SentLogMessage = "The email about the locked account was sent.";
        private const string SendFailedLogMessage = "The email about the locked account could not be sent.";
        private const string NullEmailServiceMessage = "The email service cannot be null.";
        private const string NullArgumentMessage = "The value cannot be null.";

        private readonly IEmailService _emailService;
        private readonly AppLogger _logger;

        /// <summary>
        /// Initializes the notifier.
        /// </summary>
        /// <param name="emailService">Service that delivers the emails.</param>
        /// <exception cref="ArgumentNullException">The email service is null.</exception>
        public AccountLockNotifier(IEmailService emailService)
        {
            _emailService = emailService
                ?? throw new ArgumentNullException(nameof(emailService), NullEmailServiceMessage);
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Sends the notice to the email of the player. A failure is only written in the log, because the
        /// login already tells the player on screen that the account is locked.
        /// </summary>
        /// <param name="player">Player whose account was locked.</param>
        /// <param name="culture">Language of the email.</param>
        /// <returns>True if the email was delivered to the mail server.</returns>
        /// <exception cref="ArgumentNullException">The player or the culture is null.</exception>
        public bool Send(Player player, CultureInfo culture)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player), NullArgumentMessage);
            }

            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture), NullArgumentMessage);
            }

            string body = EmailStrings.ResourceManager.GetString(BodyResourceKey, culture);
            var message = new EmailMessage
            {
                ToAddress = player.Email,
                Subject = EmailStrings.ResourceManager.GetString(SubjectResourceKey, culture),
                Body = string.Format(culture, body, player.DisplayName, LockHours)
            };

            bool isSent = EmailService.TrySend(_emailService, message, _logger);
            string logMessage = isSent ? SentLogMessage : SendFailedLogMessage;

            _logger.Info(logMessage);

            return isSent;
        }
    }
}
