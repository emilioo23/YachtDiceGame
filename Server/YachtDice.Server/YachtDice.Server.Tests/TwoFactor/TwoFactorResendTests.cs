using System;
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
        private const string UnknownEmail = "nobody@gmail.com";
        private const string DisplayName = "gabrielhernandez";
        private const string UnknownDisplayName = "nobody";
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
            var codeService = new TwoFactorCodeService();
            var deliveryService = new TwoFactorDeliveryService(codeService, _emailService);

            _players.Add(new Player
            {
                Id = PlayerId,
                Email = Email,
                PwdHash = PasswordHasher.ComputeHash(Password),
                DisplayName = DisplayName
            });
            _verificationService = new TwoFactorVerificationService(_players, codeService, deliveryService);
            _settingsService = new TwoFactorSettingsService(_players, _verificationService, deliveryService);
        }

        [Fact]
        public void ResendPlayerCode_NullRequest_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _verificationService.ResendPlayerCode(Email, null));
        }

        [Fact]
        public void ResendPlayerCode_UnknownEmail_ReturnsPlayerNotFound()
        {
            ResendCodeStatus status = _verificationService.ResendPlayerCode(UnknownEmail, CreateDelivery());

            Assert.Equal(ResendCodeStatus.PlayerNotFound, status);
        }

        [Fact]
        public void ResendPlayerCode_KnownEmail_SendsTheCodeToThatEmail()
        {
            ResendCodeStatus status = _verificationService.ResendPlayerCode(Email, CreateDelivery());

            Assert.Equal(ResendCodeStatus.CodeSent, status);
            Assert.Equal(Email, _emailService.Recipients[0]);
        }

        [Fact]
        public void ResendActivationCode_NullRequest_ThrowsArgumentNullException()
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
            ResendCodeStatus status = _settingsService.ResendActivationCode(DisplayName, CreateDelivery());

            Assert.Equal(ResendCodeStatus.CodeSent, status);
            Assert.Equal(Email, _emailService.Recipients[0]);
        }

        private static CodeDelivery CreateDelivery()
        {
            return new CodeDelivery { ChallengeId = ChallengeId, CultureName = CultureName };
        }
    }
}
