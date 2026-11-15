using System;
using Xunit;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.TwoFactor;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Authentication
{
    /// <summary>
    /// The senders of the emails of the login: the verification code and the notice of a locked account.
    /// </summary>
    public class LoginEmailsTests
    {
        private readonly InMemoryEmailService _emailService = new InMemoryEmailService();

        [Fact]
        public void Constructor_NullCodeDelivery_ThrowsArgumentNullException()
        {
            var lockNotifier = new AccountLockNotifier(_emailService);

            Assert.Throws<ArgumentNullException>(() => new LoginEmails(null, lockNotifier));
        }

        [Fact]
        public void Constructor_NullLockNotifier_ThrowsArgumentNullException()
        {
            var codeDelivery = new TwoFactorDeliveryService(new TwoFactorCodeService(), _emailService);

            Assert.Throws<ArgumentNullException>(() => new LoginEmails(codeDelivery, null));
        }

        [Fact]
        public void Constructor_BothSenders_KeepsTheLockNotifier()
        {
            var codeDelivery = new TwoFactorDeliveryService(new TwoFactorCodeService(), _emailService);
            var lockNotifier = new AccountLockNotifier(_emailService);

            var emails = new LoginEmails(codeDelivery, lockNotifier);

            Assert.Same(lockNotifier, emails.LockNotifier);
        }
    }
}
