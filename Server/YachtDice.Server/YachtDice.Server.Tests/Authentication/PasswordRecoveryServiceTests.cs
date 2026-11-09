using System;
using System.Globalization;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Authentication
{
    /// <summary>
    /// Rules of the recovery of a forgotten password with a code sent by email.
    /// </summary>
    public class PasswordRecoveryServiceTests
    {
        private const int PlayerId = 7;
        private const int HashLength = 64;
        private const int WrongAttemptsToDiscard = 3;
        private const int SecondsInsideCooldown = 5;
        private const int SecondsOutsideCooldown = 60;
        private const int MinutesExpired = 1;
        private const string Email = "emilio@example.com";
        private const string UnknownEmail = "nobody@example.com";
        private const string OldPassword = "Old123!";
        private const string NewPassword = "New456!";
        private const string CorrectCode = "123456";
        private const string WrongCode = "654321";

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryPasswordResetRepository _resets;
        private readonly InMemoryEmailService _emailService = new InMemoryEmailService();
        private readonly PasswordRecoveryService _service;

        /// <summary>
        /// Prepares the service with one registered player and without sending real emails.
        /// </summary>
        public PasswordRecoveryServiceTests()
        {
            _players.Add(new Player
            {
                Id = PlayerId,
                Email = Email,
                DisplayName = "emilio",
                PwdHash = PasswordHasher.ComputeHash(OldPassword)
            });
            _resets = new InMemoryPasswordResetRepository(_players);
            _service = CreateService(_emailService);
        }

        [Fact]
        public void RequestReset_RegisteredEmail_SavesTheHashOfTheCodeAndSendsTheEmail()
        {
            PasswordResetRequestStatus status = _service.RequestReset(Email, CultureInfo.InvariantCulture);

            Assert.Equal(PasswordResetRequestStatus.CodeSent, status);
            PwdResetToken savedToken = Assert.Single(_resets.Tokens);
            string recipient = Assert.Single(_emailService.Recipients);
            Assert.Equal(Email, recipient);
            Assert.Equal(HashLength, savedToken.Token.Length);
            Assert.True(savedToken.ExpireDate > DateTime.UtcNow);
        }

        [Fact]
        public void RequestReset_UnknownEmail_ReturnsCodeSentWithoutSavingOrSending()
        {
            PasswordResetRequestStatus status = _service.RequestReset(UnknownEmail, CultureInfo.InvariantCulture);

            Assert.Equal(PasswordResetRequestStatus.CodeSent, status);
            Assert.Empty(_resets.Tokens);
            Assert.Empty(_emailService.Recipients);
        }

        [Fact]
        public void RequestReset_BlankEmail_ReturnsMissingRequiredField()
        {
            PasswordResetRequestStatus status = _service.RequestReset("  ", CultureInfo.InvariantCulture);

            Assert.Equal(PasswordResetRequestStatus.MissingRequiredField, status);
        }

        [Fact]
        public void RequestReset_CodeRequestedMomentsAgo_DoesNotSendAnotherEmail()
        {
            SeedCode(CorrectCode, SecondsInsideCooldown);

            _service.RequestReset(Email, CultureInfo.InvariantCulture);

            Assert.Empty(_emailService.Recipients);
        }

        [Fact]
        public void RequestReset_PreviousCodeOlderThanTheCooldown_SendsANewCode()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);

            _service.RequestReset(Email, CultureInfo.InvariantCulture);

            Assert.Single(_emailService.Recipients);
        }

        [Fact]
        public void RequestReset_EmailCannotBeSent_ReturnsCodeSendFailedAndDiscardsTheCode()
        {
            PasswordRecoveryService service = CreateService(new InMemoryFailingEmailService());

            PasswordResetRequestStatus status = service.RequestReset(Email, CultureInfo.InvariantCulture);

            Assert.Equal(PasswordResetRequestStatus.CodeSendFailed, status);
            Assert.Empty(_resets.Tokens);
        }

        [Fact]
        public void RequestReset_NullCulture_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.RequestReset(Email, null));
        }

        [Fact]
        public void ConfirmReset_CorrectCode_ChangesThePasswordAndDeletesTheCode()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);

            PasswordResetStatus status = _service.ConfirmReset(CreateConfirmation(CorrectCode, NewPassword));

            Assert.Equal(PasswordResetStatus.PasswordChanged, status);
            Assert.True(PasswordHasher.Verify(NewPassword, _players.Players[0].PwdHash));
            Assert.Empty(_resets.Tokens);
        }

        [Fact]
        public void ConfirmReset_CodeAlreadyUsed_ReturnsCodeExpired()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);
            _service.ConfirmReset(CreateConfirmation(CorrectCode, NewPassword));

            PasswordResetStatus status = _service.ConfirmReset(CreateConfirmation(CorrectCode, OldPassword));

            Assert.Equal(PasswordResetStatus.CodeExpired, status);
            Assert.True(PasswordHasher.Verify(NewPassword, _players.Players[0].PwdHash));
        }

        [Fact]
        public void ConfirmReset_WrongCode_ReturnsIncorrectCodeAndKeepsThePassword()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);

            PasswordResetStatus status = _service.ConfirmReset(CreateConfirmation(WrongCode, NewPassword));

            Assert.Equal(PasswordResetStatus.IncorrectCode, status);
            Assert.True(PasswordHasher.Verify(OldPassword, _players.Players[0].PwdHash));
            Assert.Single(_resets.Tokens);
        }

        [Fact]
        public void ConfirmReset_ThreeWrongCodes_ReturnsTooManyAttemptsAndDiscardsTheCode()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);
            PasswordResetStatus status = PasswordResetStatus.IncorrectCode;

            for (var attempt = 0; attempt < WrongAttemptsToDiscard; attempt++)
            {
                status = _service.ConfirmReset(CreateConfirmation(WrongCode, NewPassword));
            }

            Assert.Equal(PasswordResetStatus.TooManyAttempts, status);
            Assert.Empty(_resets.Tokens);
        }

        [Fact]
        public void ConfirmReset_CorrectCodeAfterTooManyAttempts_ReturnsCodeExpired()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);

            for (var attempt = 0; attempt < WrongAttemptsToDiscard; attempt++)
            {
                _service.ConfirmReset(CreateConfirmation(WrongCode, NewPassword));
            }

            PasswordResetStatus status = _service.ConfirmReset(CreateConfirmation(CorrectCode, NewPassword));

            Assert.Equal(PasswordResetStatus.CodeExpired, status);
            Assert.True(PasswordHasher.Verify(OldPassword, _players.Players[0].PwdHash));
        }

        [Fact]
        public void ConfirmReset_ExpiredCode_ReturnsCodeExpiredAndDeletesIt()
        {
            _resets.Save(PlayerId, PasswordResetCodeHasher.Compute(PlayerId, CorrectCode), ExpiredDate());

            PasswordResetStatus status = _service.ConfirmReset(CreateConfirmation(CorrectCode, NewPassword));

            Assert.Equal(PasswordResetStatus.CodeExpired, status);
            Assert.Empty(_resets.Tokens);
        }

        [Fact]
        public void ConfirmReset_NoCodeRequested_ReturnsCodeExpired()
        {
            PasswordResetStatus status = _service.ConfirmReset(CreateConfirmation(CorrectCode, NewPassword));

            Assert.Equal(PasswordResetStatus.CodeExpired, status);
        }

        [Fact]
        public void ConfirmReset_UnknownEmail_ReturnsCodeExpired()
        {
            var confirmation = new PasswordResetConfirmation
            {
                Email = UnknownEmail,
                Code = CorrectCode,
                NewPassword = NewPassword
            };

            PasswordResetStatus status = _service.ConfirmReset(confirmation);

            Assert.Equal(PasswordResetStatus.CodeExpired, status);
        }

        [Theory]
        [InlineData("", NewPassword)]
        [InlineData(CorrectCode, "")]
        [InlineData(CorrectCode, "   ")]
        public void ConfirmReset_BlankCodeOrPassword_ReturnsMissingRequiredField(string code, string newPassword)
        {
            PasswordResetStatus status = _service.ConfirmReset(CreateConfirmation(code, newPassword));

            Assert.Equal(PasswordResetStatus.MissingRequiredField, status);
        }

        private static DateTime ExpiredDate()
        {
            return DateTime.UtcNow.AddMinutes(-MinutesExpired);
        }

        private static PasswordResetConfirmation CreateConfirmation(string code, string newPassword)
        {
            return new PasswordResetConfirmation
            {
                Email = Email,
                Code = code,
                NewPassword = newPassword
            };
        }

        private PasswordRecoveryService CreateService(IEmailService emailService)
        {
            return new PasswordRecoveryService(_players, _resets, new PasswordResetEmailSender(emailService));
        }

        private void SeedCode(string code, int secondsSinceRequest)
        {
            DateTime createdAt = DateTime.UtcNow.AddSeconds(-secondsSinceRequest);
            DateTime expireDate = createdAt.AddMinutes(PasswordRecoveryService.ExpirationMinutes);

            _resets.Save(PlayerId, PasswordResetCodeHasher.Compute(PlayerId, code), expireDate);
        }
    }
}
