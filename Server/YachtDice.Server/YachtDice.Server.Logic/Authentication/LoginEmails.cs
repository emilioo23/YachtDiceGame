using System;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.TwoFactor;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// The emails that the login can send: the verification code of the two-step verification and the notice
    /// of an account locked for the day.
    /// </summary>
    public sealed class LoginEmails
    {
        private const string NullSenderMessage = "A sender of the login emails cannot be null.";

        /// <summary>
        /// Initializes the senders of the login.
        /// </summary>
        /// <param name="codeDelivery">Sends the verification code of the two-step verification.</param>
        /// <param name="lockNotifier">Tells the player that the account was locked for the day.</param>
        /// <exception cref="ArgumentNullException">One of the senders is null.</exception>
        public LoginEmails(TwoFactorDeliveryService codeDelivery, AccountLockNotifier lockNotifier)
        {
            CodeDelivery = codeDelivery ?? throw new ArgumentNullException(nameof(codeDelivery), NullSenderMessage);
            LockNotifier = lockNotifier ?? throw new ArgumentNullException(nameof(lockNotifier), NullSenderMessage);
        }

        /// <summary>
        /// Gets the sender of the verification code.
        /// </summary>
        public TwoFactorDeliveryService CodeDelivery { get; }

        /// <summary>
        /// Gets the sender of the notice of a locked account.
        /// </summary>
        public AccountLockNotifier LockNotifier { get; }
    }
}
