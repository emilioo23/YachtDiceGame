using System;
using System.Globalization;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.Resources;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Email
{
    /// <summary>
    /// The email that sends a code to a player in the language of the player: it carries the name, the code and
    /// how long the code lasts, and a failure of the mail server only makes it return false.
    /// </summary>
    public class CodeEmailSenderTests
    {
        private const string Email = "gabriel.hernandez@gmail.com";
        private const string DisplayName = "gabrielhernandez";
        private const string Code = "482913";
        private const string SubjectKey = "TwoFactor_EmailSubject";
        private const string BodyKey = "TwoFactor_EmailBody";
        private const int ExpirationMinutes = 10;
        private const string SpanishCultureName = "es-MX";
        private const string EnglishCultureName = "en-US";

        private static readonly CultureInfo SpanishCulture = new CultureInfo(SpanishCultureName);
        private static readonly CultureInfo EnglishCulture = new CultureInfo(EnglishCultureName);

        private readonly InMemoryEmailService _emailService = new InMemoryEmailService();
        private readonly CodeEmailTemplate _template = new CodeEmailTemplate(SubjectKey, BodyKey, ExpirationMinutes);
        private readonly Player _player = new Player { Email = Email, DisplayName = DisplayName };

        [Fact]
        public void Constructor_NullEmailService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CodeEmailSender(null, _template));
        }

        [Fact]
        public void Constructor_NullTemplate_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CodeEmailSender(_emailService, null));
        }

        [Fact]
        public void Send_NullPlayer_ThrowsArgumentNullException()
        {
            var sender = new CodeEmailSender(_emailService, _template);

            Assert.Throws<ArgumentNullException>(() => sender.Send(null, Code, SpanishCulture));
        }

        [Fact]
        public void Send_NullCulture_ThrowsArgumentNullException()
        {
            var sender = new CodeEmailSender(_emailService, _template);

            Assert.Throws<ArgumentNullException>(() => sender.Send(_player, Code, null));
        }

        [Fact]
        public void Send_WorkingEmailService_ReturnsTrue()
        {
            var sender = new CodeEmailSender(_emailService, _template);

            bool isSent = sender.Send(_player, Code, SpanishCulture);

            Assert.True(isSent);
        }

        [Fact]
        public void Send_WorkingEmailService_SendsTheEmailToTheAddressOfThePlayer()
        {
            var sender = new CodeEmailSender(_emailService, _template);

            sender.Send(_player, Code, SpanishCulture);

            Assert.Equal(Email, _emailService.LastMessage.ToAddress);
        }

        [Fact]
        public void Send_WorkingEmailService_PutsTheCodeInTheBody()
        {
            var sender = new CodeEmailSender(_emailService, _template);

            sender.Send(_player, Code, SpanishCulture);

            Assert.Contains(Code, _emailService.LastMessage.Body);
        }

        [Fact]
        public void Send_WorkingEmailService_PutsTheNameOfThePlayerInTheBody()
        {
            var sender = new CodeEmailSender(_emailService, _template);

            sender.Send(_player, Code, SpanishCulture);

            Assert.Contains(DisplayName, _emailService.LastMessage.Body);
        }

        [Fact]
        public void Send_WorkingEmailService_PutsTheMinutesThatTheCodeLastsInTheBody()
        {
            string minutesText = ExpirationMinutes.ToString(CultureInfo.InvariantCulture);
            var sender = new CodeEmailSender(_emailService, _template);

            sender.Send(_player, Code, SpanishCulture);

            Assert.Contains(minutesText, _emailService.LastMessage.Body);
        }

        [Fact]
        public void Send_SpanishCulture_UsesTheSubjectInSpanish()
        {
            string subjectExpected = EmailStrings.ResourceManager.GetString(SubjectKey, SpanishCulture);
            var sender = new CodeEmailSender(_emailService, _template);

            sender.Send(_player, Code, SpanishCulture);

            Assert.Equal(subjectExpected, _emailService.LastMessage.Subject);
        }

        [Fact]
        public void Send_EnglishCulture_UsesTheSubjectInEnglish()
        {
            string subjectExpected = EmailStrings.ResourceManager.GetString(SubjectKey, EnglishCulture);
            var sender = new CodeEmailSender(_emailService, _template);

            sender.Send(_player, Code, EnglishCulture);

            Assert.Equal(subjectExpected, _emailService.LastMessage.Subject);
        }

        [Fact]
        public void Send_EnglishCulture_DoesNotUseTheSpanishSubject()
        {
            string spanishSubject = EmailStrings.ResourceManager.GetString(SubjectKey, SpanishCulture);
            var sender = new CodeEmailSender(_emailService, _template);

            sender.Send(_player, Code, EnglishCulture);

            Assert.NotEqual(spanishSubject, _emailService.LastMessage.Subject);
        }

        [Fact]
        public void Send_FailingEmailService_ReturnsFalse()
        {
            var sender = new CodeEmailSender(new InMemoryFailingEmailService(), _template);

            bool isSent = sender.Send(_player, Code, SpanishCulture);

            Assert.False(isSent);
        }
    }
}
