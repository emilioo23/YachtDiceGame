using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Email
{
    /// <summary>
    /// The email that tells a player that the account was locked for the day.
    /// </summary>
    public class AccountLockNotifierTests
    {
        private const string Email = "gabriel.hernandez@gmail.com";
        private const string DisplayName = "gabrielhernandez";
        private const string SpanishCultureName = "es-MX";

        private static readonly CultureInfo SpanishCulture = new CultureInfo(SpanishCultureName);

        private readonly InMemoryEmailService _emailService = new InMemoryEmailService();
        private readonly Player _player = new Player { Email = Email, DisplayName = DisplayName };

        [Fact]
        public void Send_WorkingEmailService_SendsTheNoticeToThePlayer()
        {
            var notifier = new AccountLockNotifier(_emailService);
            var notificationExpected = new { IsSent = true, Recipients = new List<string> { Email } };

            bool isSent = notifier.Send(_player, SpanishCulture);

            Assert.Equivalent(notificationExpected, new { IsSent = isSent, _emailService.Recipients });
        }

        [Fact]
        public void Send_FailingEmailService_ReturnsFalse()
        {
            var notifier = new AccountLockNotifier(new InMemoryFailingEmailService());

            bool isSent = notifier.Send(_player, SpanishCulture);

            Assert.False(isSent);
        }

        [Fact]
        public void Send_NullPlayer_ThrowsArgumentNullException()
        {
            var notifier = new AccountLockNotifier(_emailService);

            Assert.Throws<ArgumentNullException>(() => notifier.Send(null, SpanishCulture));
        }

        [Fact]
        public void Send_NullCulture_ThrowsArgumentNullException()
        {
            var notifier = new AccountLockNotifier(_emailService);

            Assert.Throws<ArgumentNullException>(() => notifier.Send(_player, null));
        }

        [Fact]
        public void Constructor_NullEmailService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AccountLockNotifier(null));
        }
    }
}
