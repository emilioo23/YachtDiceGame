using System;
using System.Net.Mail;
using YachtDice.Server.Logic.Logging;

namespace YachtDice.Server.Logic.Email
{
    /// <summary>
    /// Sends one email and turns the failures of the mail server into a false result. The error is written in the
    /// log of the class that sends the email, and that class decides what the player is told. Other errors are
    /// programming errors, so they are not caught here.
    /// </summary>
    internal static class EmailDelivery
    {
        private const string SendFailedLogMessage = "An email could not be delivered to the mail server.";

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="emailService">Service that delivers the emails.</param>
        /// <param name="message">Email to send.</param>
        /// <param name="logger">Logger of the class that sends the email.</param>
        /// <returns>True if the email was delivered to the mail server.</returns>
        internal static bool TrySend(IEmailService emailService, EmailMessage message, AppLogger logger)
        {
            var isSent = false;

            try
            {
                emailService.Send(message);
                isSent = true;
            }
            catch (Exception ex) when (IsDeliveryFailure(ex))
            {
                logger.Error(ex, SendFailedLogMessage);
            }

            return isSent;
        }

        private static bool IsDeliveryFailure(Exception exception)
        {
            bool isMailFailure = exception is SmtpException || exception is InvalidOperationException;

            return isMailFailure || exception is FormatException || exception is ArgumentException;
        }
    }
}
