using System;
using System.Globalization;
using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Authentication
{
    /// <summary>
    /// Rules of the recovery of a forgotten password with a code sent by email. Each scenario has one test for
    /// the status that the service returns and, when the scenario changes something, another one for that change.
    /// </summary>
    public class PasswordRecoveryServiceTests
    {
        private const int PlayerId = 7;
        private const int HashLength = 64;
        private const int WrongAttemptsToDiscard = 3;
        private const int SecondsInsideCooldown = 5;
        private const int SecondsOutsideCooldown = 60;
        private const int MinutesExpired = 1;
        private const int NoTokens = 0;
        private const int OneToken = 1;
        private const int NoRecipients = 0;
        private const string Email = "emilio@example.com";
        private const string UnknownEmail = "nobody@example.com";
        private const string BlankEmail = "  ";
        private const string DisplayName = "emilio";
        private const string OldPassword = "OldPass123!";
        private const string NewPassword = "NewPass456!";
        private const string WeakPassword = "abc";
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
                DisplayName = DisplayName,
                PwdHash = PasswordHasher.ComputeHash(OldPassword)
            });
            _resets = new InMemoryPasswordResetRepository(_players);
            _service = CreateService(_emailService);
        }

        [Fact]
        public void RequestReset_RegisteredEmail_ReturnsCodeSent()
        {
            PasswordResetRequestStatus status = _service.RequestReset(Email, CultureInfo.InvariantCulture);

            Assert.Equal(PasswordResetRequestStatus.CodeSent, status);
        }

        [Fact]
        public void RequestReset_RegisteredEmail_SavesTheHashOfTheCodeAndSendsTheEmail()
        {
            var expectedRecovery = new
            {
                TokenCount = OneToken,
                TokenLength = HashLength,
                IsStillValid = true,
                Recipients = new[] { Email }
            };

            _service.RequestReset(Email, CultureInfo.InvariantCulture);

            PwdResetToken savedToken = _resets.Tokens.First();
            var actualRecovery = new
            {
                TokenCount = _resets.Tokens.Count,
                TokenLength = savedToken.Token.Length,
                IsStillValid = savedToken.ExpireDate > DateTime.UtcNow,
                Recipients = _emailService.Recipients.ToArray()
            };
            Assert.Equivalent(expectedRecovery, actualRecovery);
        }

        [Fact]
        public void RequestReset_UnknownEmail_ReturnsCodeSent()
        {
            PasswordResetRequestStatus status = _service.RequestReset(UnknownEmail, CultureInfo.InvariantCulture);

            Assert.Equal(PasswordResetRequestStatus.CodeSent, status);
        }

        [Fact]
        public void RequestReset_UnknownEmail_DoesNotSaveOrSendAnything()
        {
            var expectedRecovery = new { TokenCount = NoTokens, RecipientCount = NoRecipients };

            _service.RequestReset(UnknownEmail, CultureInfo.InvariantCulture);

            var actualRecovery = new
            {
                TokenCount = _resets.Tokens.Count,
                RecipientCount = _emailService.Recipients.Count
            };
            Assert.Equal(expectedRecovery, actualRecovery);
        }

        [Fact]
        public void RequestReset_BlankEmail_ReturnsMissingRequiredField()
        {
            PasswordResetRequestStatus status = _service.RequestReset(BlankEmail, CultureInfo.InvariantCulture);

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
        public void RequestReset_EmailCannotBeSent_ReturnsCodeSendFailed()
        {
            PasswordRecoveryService service = CreateService(new InMemoryFailingEmailService());

            PasswordResetRequestStatus status = service.RequestReset(Email, CultureInfo.InvariantCulture);

            Assert.Equal(PasswordResetRequestStatus.CodeSendFailed, status);
        }

        [Fact]
        public void RequestReset_EmailCannotBeSent_DiscardsTheCode()
        {
            PasswordRecoveryService service = CreateService(new InMemoryFailingEmailService());

            service.RequestReset(Email, CultureInfo.InvariantCulture);

            Assert.Empty(_resets.Tokens);
        }

        [Fact]
        public void RequestReset_NullCulture_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.RequestReset(Email, null));
        }

        [Fact]
        public void ConfirmReset_CorrectCode_ReturnsPasswordChanged()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);

            PasswordResetStatus status = _service.ConfirmReset(CreateConfirmation(CorrectCode, NewPassword));

            Assert.Equal(PasswordResetStatus.PasswordChanged, status);
        }

        [Fact]
        public void ConfirmReset_CorrectCode_ChangesThePasswordAndDeletesTheCode()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);
            var expectedReset = new { HasNewPassword = true, TokenCount = NoTokens };

            _service.ConfirmReset(CreateConfirmation(CorrectCode, NewPassword));

            var actualReset = new
            {
                HasNewPassword = IsCurrentPassword(NewPassword),
                TokenCount = _resets.Tokens.Count
            };
            Assert.Equal(expectedReset, actualReset);
        }

        [Fact]
        public void ConfirmReset_CodeAlreadyUsed_ReturnsCodeExpired()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);
            _service.ConfirmReset(CreateConfirmation(CorrectCode, NewPassword));

            PasswordResetStatus status = _service.ConfirmReset(CreateConfirmation(CorrectCode, OldPassword));

            Assert.Equal(PasswordResetStatus.CodeExpired, status);
        }

        [Fact]
        public void ConfirmReset_CodeAlreadyUsed_KeepsThePasswordOfTheFirstChange()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);
            _service.ConfirmReset(CreateConfirmation(CorrectCode, NewPassword));

            _service.ConfirmReset(CreateConfirmation(CorrectCode, OldPassword));

            Assert.True(IsCurrentPassword(NewPassword));
        }

        [Fact]
        public void ConfirmReset_WrongCode_ReturnsIncorrectCode()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);

            PasswordResetStatus status = _service.ConfirmReset(CreateConfirmation(WrongCode, NewPassword));

            Assert.Equal(PasswordResetStatus.IncorrectCode, status);
        }

        [Fact]
        public void ConfirmReset_WrongCode_KeepsThePasswordAndTheCode()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);
            var expectedReset = new { HasOldPassword = true, TokenCount = OneToken };

            _service.ConfirmReset(CreateConfirmation(WrongCode, NewPassword));

            var actualReset = new
            {
                HasOldPassword = IsCurrentPassword(OldPassword),
                TokenCount = _resets.Tokens.Count
            };
            Assert.Equal(expectedReset, actualReset);
        }

        [Fact]
        public void ConfirmReset_ThreeWrongCodes_ReturnsTooManyAttempts()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);

            PasswordResetStatus status = TryWrongCodeUntilDiscarded();

            Assert.Equal(PasswordResetStatus.TooManyAttempts, status);
        }

        [Fact]
        public void ConfirmReset_ThreeWrongCodes_DiscardsTheCode()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);

            TryWrongCodeUntilDiscarded();

            Assert.Empty(_resets.Tokens);
        }

        [Fact]
        public void ConfirmReset_CorrectCodeAfterTooManyAttempts_ReturnsCodeExpired()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);
            TryWrongCodeUntilDiscarded();

            PasswordResetStatus status = _service.ConfirmReset(CreateConfirmation(CorrectCode, NewPassword));

            Assert.Equal(PasswordResetStatus.CodeExpired, status);
        }

        [Fact]
        public void ConfirmReset_CorrectCodeAfterTooManyAttempts_KeepsTheOldPassword()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);
            TryWrongCodeUntilDiscarded();

            _service.ConfirmReset(CreateConfirmation(CorrectCode, NewPassword));

            Assert.True(IsCurrentPassword(OldPassword));
        }

        [Fact]
        public void ConfirmReset_ExpiredCode_ReturnsCodeExpired()
        {
            SeedExpiredCode();

            PasswordResetStatus status = _service.ConfirmReset(CreateConfirmation(CorrectCode, NewPassword));

            Assert.Equal(PasswordResetStatus.CodeExpired, status);
        }

        [Fact]
        public void ConfirmReset_ExpiredCode_DeletesTheCode()
        {
            SeedExpiredCode();

            _service.ConfirmReset(CreateConfirmation(CorrectCode, NewPassword));

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

        [Fact]
        public void ConfirmReset_WeakNewPassword_ReturnsWeakPassword()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);

            PasswordResetStatus status = _service.ConfirmReset(CreateConfirmation(CorrectCode, WeakPassword));

            Assert.Equal(PasswordResetStatus.WeakPassword, status);
        }

        [Fact]
        public void ConfirmReset_WeakNewPassword_KeepsTheOldPassword()
        {
            SeedCode(CorrectCode, SecondsOutsideCooldown);

            _service.ConfirmReset(CreateConfirmation(CorrectCode, WeakPassword));

            Assert.True(IsCurrentPassword(OldPassword));
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
            return new PasswordRecoveryService(_players, _resets, emailService);
        }

        private bool IsCurrentPassword(string password)
        {
            return PasswordHasher.Verify(password, _players.Players[0].PwdHash);
        }

        private PasswordResetStatus TryWrongCodeUntilDiscarded()
        {
            PasswordResetStatus status = PasswordResetStatus.IncorrectCode;

            for (var attempt = 0; attempt < WrongAttemptsToDiscard; attempt++)
            {
                status = _service.ConfirmReset(CreateConfirmation(WrongCode, NewPassword));
            }

            return status;
        }

        private void SeedCode(string code, int secondsSinceRequest)
        {
            DateTime createdAt = DateTime.UtcNow.AddSeconds(-secondsSinceRequest);
            DateTime expireDate = createdAt.AddMinutes(PasswordRecoveryService.ExpirationMinutes);

            _resets.Save(CreateToken(code, expireDate));
        }

        private void SeedExpiredCode()
        {
            _resets.Save(CreateToken(CorrectCode, ExpiredDate()));
        }

        private static PwdResetToken CreateToken(string code, DateTime expireDate)
        {
            return new PwdResetToken
            {
                PlayerId = PlayerId,
                Token = PasswordResetCodeHasher.Compute(PlayerId, code),
                ExpireDate = expireDate
            };
        }
    }
}
