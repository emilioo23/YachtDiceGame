using System;
using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Logic.TwoFactor;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.TwoFactor
{
    /// <summary>
    /// Sending a new code to a player who is verifying the login or turning the verification on.
    /// </summary>
    public class TwoFactorResendTests
    {
        private const int PlayerId = 1;
        private const string Email = "gabriel@gmail.com";
        private const string UnknownEmail = "carlos.ramirez@gmail.com";
        private const string DisplayName = "gabrielhernandez";
        private const string UnknownDisplayName = "carlosramirez";
        private const string Password = "Secret123!";
        private const string ChallengeId = "challenge-1";
        private const string CultureName = "es-MX";

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryEmailService _emailService = new InMemoryEmailService();
        private readonly TwoFactorVerificationService _verificationService;
        private readonly TwoFactorSettingsService _settingsService;

        /// <summary>
        /// Prepares the services with one registered player and without sending real emails.
        /// </summary>
        public TwoFactorResendTests()
        {
            var codeService = new TwoFactorCodeService(new InMemoryBackupCodeRepository(), _emailService);

            _players.Add(new Player
            {
                Id = PlayerId,
                Email = Email,
                PwdHash = CredentialRules.ComputeHash(Password),
                DisplayName = DisplayName
            });
            _verificationService = new TwoFactorVerificationService(_players, codeService);
            _settingsService = new TwoFactorSettingsService(_players, _verificationService, codeService);
        }

        [Fact]
        public void ResendPlayerCode_NullDelivery_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _verificationService.ResendPlayerCode(Email, null));
        }

        [Fact]
        public void ResendPlayerCode_UnknownEmail_ReturnsCodeSentWithoutSendingAnything()
        {
            var outcomeExpected = new { Status = ResendCodeStatus.CodeSent, SentCount = 0 };

            ResendCodeStatus status = _verificationService.ResendPlayerCode(UnknownEmail, CreateDelivery());

            var outcomeResult = new { Status = status, SentCount = _emailService.Recipients.Count };
            Assert.Equal(outcomeExpected, outcomeResult);
        }

        [Fact]
        public void ResendPlayerCode_ChallengeOfAnotherPlayer_ReturnsCodeSentWithoutSendingAnything()
        {
            var outcomeExpected = new { Status = ResendCodeStatus.CodeSent, SentCount = 0 };
            CodeDelivery delivery = CreateDelivery();
            delivery.OwnerName = UnknownDisplayName;

            ResendCodeStatus status = _verificationService.ResendPlayerCode(Email, delivery);

            var outcomeResult = new { Status = status, SentCount = _emailService.Recipients.Count };
            Assert.Equal(outcomeExpected, outcomeResult);
        }

        [Fact]
        public void ResendPlayerCode_ChallengeNotIssuedByTheServer_ReturnsCodeSentWithoutSendingAnything()
        {
            var outcomeExpected = new { Status = ResendCodeStatus.CodeSent, SentCount = 0 };
            CodeDelivery delivery = CreateDelivery();
            delivery.OwnerName = string.Empty;

            ResendCodeStatus status = _verificationService.ResendPlayerCode(Email, delivery);

            var outcomeResult = new { Status = status, SentCount = _emailService.Recipients.Count };
            Assert.Equal(outcomeExpected, outcomeResult);
        }

        [Fact]
        public void ResendPlayerCode_KnownEmail_SendsTheCodeToThatEmail()
        {
            var outcomeExpected = new { Status = ResendCodeStatus.CodeSent, Recipient = Email };

            ResendCodeStatus status = _verificationService.ResendPlayerCode(Email, CreateDelivery());

            var outcomeResult = new { Status = status, Recipient = _emailService.Recipients.First() };
            Assert.Equal(outcomeExpected, outcomeResult);
        }

        [Fact]
        public void ResendActivationCode_NullDelivery_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _settingsService.ResendActivationCode(DisplayName, null));
        }

        [Fact]
        public void ResendActivationCode_UnknownPlayer_ReturnsPlayerNotFound()
        {
            ResendCodeStatus status = _settingsService.ResendActivationCode(UnknownDisplayName, CreateDelivery());

            Assert.Equal(ResendCodeStatus.PlayerNotFound, status);
        }

        [Fact]
        public void ResendActivationCode_KnownPlayer_SendsTheCodeToTheEmailOfThePlayer()
        {
            var outcomeExpected = new { Status = ResendCodeStatus.CodeSent, Recipient = Email };

            ResendCodeStatus status = _settingsService.ResendActivationCode(DisplayName, CreateDelivery());

            var outcomeResult = new { Status = status, Recipient = _emailService.Recipients.First() };
            Assert.Equal(outcomeExpected, outcomeResult);
        }

        private static CodeDelivery CreateDelivery()
        {
            return new CodeDelivery { ChallengeId = ChallengeId, CultureName = CultureName, OwnerName = DisplayName };
        }
    }
}
