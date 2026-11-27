using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Logic.TwoFactor;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.TwoFactor
{
    /// <summary>
    /// The codes of the two-step verification: each code belongs to one challenge, is used only once, and three
    /// wrong codes in a row lock the verification for a while, even if a new code is asked for.
    /// </summary>
    public class TwoFactorCodeServiceTests
    {
        private const string Email = "gabriel.hernandez@gmail.com";
        private const string OtherEmail = "emilio.alvarez@gmail.com";
        private const string DisplayName = "gabrielhernandez";
        private const string ChallengeId = "challenge-1";
        private const string OtherChallengeId = "challenge-2";
        private const string WrongCode = "000000";
        private const string ShortBackupCode = "1234567";
        private const string BackupCodeWithLetter = "1234567a";
        private const string ValidBackupCode = "12345678";
        private const string SpanishCultureName = "es-MX";
        private const int PlayerId = 7;
        private const int NoBackupCodes = 0;
        private const int AttemptsBeforeTheLock = SecureCodes.MaxFailedAttempts - 1;

        private static readonly CultureInfo SpanishCulture = new CultureInfo(SpanishCultureName);

        private readonly InMemoryBackupCodeRepository _backupCodes = new InMemoryBackupCodeRepository();
        private readonly InMemoryEmailService _emailService = new InMemoryEmailService();
        private readonly TwoFactorCodeService _service;

        /// <summary>
        /// Prepares the service with backup codes kept in memory and without sending real emails.
        /// </summary>
        public TwoFactorCodeServiceTests()
        {
            _service = new TwoFactorCodeService(_backupCodes, _emailService);
        }

        [Fact]
        public void Constructor_NullBackupCodeRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TwoFactorCodeService(null, _emailService));
        }

        [Fact]
        public void Constructor_NullEmailService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TwoFactorCodeService(_backupCodes, null));
        }

        [Fact]
        public void GenerateCode_AnyEmail_ReturnsACodeOfTheRightLength()
        {
            string code = _service.GenerateCode(Email, ChallengeId);

            Assert.Equal(SecureCodes.CodeLength, code.Length);
        }

        [Fact]
        public void GenerateCode_AnyEmail_ReturnsOnlyDigits()
        {
            string code = _service.GenerateCode(Email, ChallengeId);

            Assert.True(code.All(char.IsDigit));
        }

        [Fact]
        public void ValidateCode_RightCode_ReturnsValid()
        {
            string code = _service.GenerateCode(Email, ChallengeId);

            TwoFactorValidationStatus status = _service.ValidateCode(Email, CreateAnswer(code, ChallengeId));

            Assert.Equal(TwoFactorValidationStatus.Valid, status);
        }

        [Fact]
        public void ValidateCode_WrongCode_ReturnsIncorrect()
        {
            _service.GenerateCode(Email, ChallengeId);

            TwoFactorValidationStatus status = _service.ValidateCode(Email, CreateAnswer(WrongCode, ChallengeId));

            Assert.Equal(TwoFactorValidationStatus.Incorrect, status);
        }

        [Fact]
        public void ValidateCode_RightCodeUsedTwice_ReturnsExpiredTheSecondTime()
        {
            string code = _service.GenerateCode(Email, ChallengeId);
            _service.ValidateCode(Email, CreateAnswer(code, ChallengeId));

            TwoFactorValidationStatus status = _service.ValidateCode(Email, CreateAnswer(code, ChallengeId));

            Assert.Equal(TwoFactorValidationStatus.Expired, status);
        }

        [Fact]
        public void ValidateCode_EmailWithoutACode_ReturnsExpired()
        {
            TwoFactorValidationStatus status = _service.ValidateCode(Email, CreateAnswer(WrongCode, ChallengeId));

            Assert.Equal(TwoFactorValidationStatus.Expired, status);
        }

        [Fact]
        public void ValidateCode_RightCodeOfAnotherEmail_ReturnsExpired()
        {
            string code = _service.GenerateCode(OtherEmail, ChallengeId);

            TwoFactorValidationStatus status = _service.ValidateCode(Email, CreateAnswer(code, ChallengeId));

            Assert.Equal(TwoFactorValidationStatus.Expired, status);
        }

        [Fact]
        public void ValidateCode_RightCodeWithAnotherChallenge_ReturnsExpired()
        {
            string code = _service.GenerateCode(Email, ChallengeId);

            TwoFactorValidationStatus status = _service.ValidateCode(Email, CreateAnswer(code, OtherChallengeId));

            Assert.Equal(TwoFactorValidationStatus.Expired, status);
        }

        [Fact]
        public void ValidateCode_CodeReplacedByANewOne_ReturnsIncorrectForTheOldCode()
        {
            string oldCode = _service.GenerateCode(Email, ChallengeId);
            ReplaceWithDifferentCode(oldCode);

            TwoFactorValidationStatus status = _service.ValidateCode(Email, CreateAnswer(oldCode, ChallengeId));

            Assert.Equal(TwoFactorValidationStatus.Incorrect, status);
        }

        [Fact]
        public void ValidateCode_LastWrongCodeAllowed_ReturnsLocked()
        {
            _service.GenerateCode(Email, ChallengeId);
            FailTimes(AttemptsBeforeTheLock);

            TwoFactorValidationStatus status = _service.ValidateCode(Email, CreateAnswer(WrongCode, ChallengeId));

            Assert.Equal(TwoFactorValidationStatus.Locked, status);
        }

        [Fact]
        public void ValidateCode_RightCodeAfterTheLock_ReturnsLocked()
        {
            string code = _service.GenerateCode(Email, ChallengeId);
            FailTimes(SecureCodes.MaxFailedAttempts);

            TwoFactorValidationStatus status = _service.ValidateCode(Email, CreateAnswer(code, ChallengeId));

            Assert.Equal(TwoFactorValidationStatus.Locked, status);
        }

        [Fact]
        public void ValidateCode_NullAnswer_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.ValidateCode(Email, null));
        }

        [Fact]
        public void IsLocked_EmailWithoutACode_ReturnsFalse()
        {
            bool isLocked = _service.IsLocked(Email);

            Assert.False(isLocked);
        }

        [Fact]
        public void IsLocked_FewerWrongCodesThanTheMaximum_ReturnsFalse()
        {
            _service.GenerateCode(Email, ChallengeId);
            FailTimes(AttemptsBeforeTheLock);

            bool isLocked = _service.IsLocked(Email);

            Assert.False(isLocked);
        }

        [Fact]
        public void IsLocked_MaximumWrongCodes_ReturnsTrue()
        {
            _service.GenerateCode(Email, ChallengeId);
            FailTimes(SecureCodes.MaxFailedAttempts);

            bool isLocked = _service.IsLocked(Email);

            Assert.True(isLocked);
        }

        [Fact]
        public void IsLocked_NewCodeAskedDuringTheLock_KeepsTheLock()
        {
            _service.GenerateCode(Email, ChallengeId);
            FailTimes(SecureCodes.MaxFailedAttempts);
            _service.GenerateCode(Email, OtherChallengeId);

            bool isLocked = _service.IsLocked(Email);

            Assert.True(isLocked);
        }

        [Fact]
        public void IsLocked_AnotherEmailLocked_ReturnsFalse()
        {
            _service.GenerateCode(OtherEmail, ChallengeId);
            FailTimesFor(OtherEmail, SecureCodes.MaxFailedAttempts);

            bool isLocked = _service.IsLocked(Email);

            Assert.False(isLocked);
        }

        [Fact]
        public void IsResendAllowed_CodeJustSent_ReturnsFalse()
        {
            _service.GenerateCode(Email, ChallengeId);

            bool isAllowed = _service.IsResendAllowed(Email, ChallengeId);

            Assert.False(isAllowed);
        }

        [Fact]
        public void IsResendAllowed_EmailWithoutACode_ReturnsTrue()
        {
            bool isAllowed = _service.IsResendAllowed(Email, ChallengeId);

            Assert.True(isAllowed);
        }

        [Fact]
        public void IsResendAllowed_CodeOfAnotherChallenge_ReturnsTrue()
        {
            _service.GenerateCode(Email, ChallengeId);

            bool isAllowed = _service.IsResendAllowed(Email, OtherChallengeId);

            Assert.True(isAllowed);
        }

        [Fact]
        public void CreateNewChallengeId_TwoCalls_ReturnsDifferentIdentifiers()
        {
            string firstId = _service.CreateNewChallengeId();

            string secondId = _service.CreateNewChallengeId();

            Assert.NotEqual(firstId, secondId);
        }

        [Fact]
        public void IsBackupCodeFormat_EightDigits_ReturnsTrue()
        {
            bool isBackupCode = TwoFactorCodeService.IsBackupCodeFormat(ValidBackupCode);

            Assert.True(isBackupCode);
        }

        [Fact]
        public void IsBackupCodeFormat_SevenDigits_ReturnsFalse()
        {
            bool isBackupCode = TwoFactorCodeService.IsBackupCodeFormat(ShortBackupCode);

            Assert.False(isBackupCode);
        }

        [Fact]
        public void IsBackupCodeFormat_CodeWithALetter_ReturnsFalse()
        {
            bool isBackupCode = TwoFactorCodeService.IsBackupCodeFormat(BackupCodeWithLetter);

            Assert.False(isBackupCode);
        }

        [Fact]
        public void IsBackupCodeFormat_NullCode_ReturnsFalse()
        {
            bool isBackupCode = TwoFactorCodeService.IsBackupCodeFormat(null);

            Assert.False(isBackupCode);
        }

        [Fact]
        public void CreateBackupCodes_AnyPlayer_ReturnsTheConfiguredNumberOfCodes()
        {
            List<string> codes = _service.CreateBackupCodes(PlayerId);

            Assert.Equal(TwoFactorCodeService.BackupCodeCount, codes.Count);
        }

        [Fact]
        public void CreateBackupCodes_AnyPlayer_ReturnsCodesInTheBackupFormat()
        {
            List<string> codes = _service.CreateBackupCodes(PlayerId);

            Assert.True(codes.All(TwoFactorCodeService.IsBackupCodeFormat));
        }

        [Fact]
        public void CountBackupCodes_AfterCreatingThem_ReturnsAllTheCodes()
        {
            _service.CreateBackupCodes(PlayerId);

            int count = _service.CountBackupCodes(PlayerId);

            Assert.Equal(TwoFactorCodeService.BackupCodeCount, count);
        }

        [Fact]
        public void RemoveBackupCodes_PlayerWithCodes_LeavesNoCodes()
        {
            _service.CreateBackupCodes(PlayerId);

            _service.RemoveBackupCodes(PlayerId);
            int count = _service.CountBackupCodes(PlayerId);

            Assert.Equal(NoBackupCodes, count);
        }

        [Fact]
        public void ValidateBackupCode_ActiveCode_ReturnsValid()
        {
            string backupCode = _service.CreateBackupCodes(PlayerId).First();
            _service.GenerateCode(Email, ChallengeId);

            TwoFactorValidationStatus status =
                _service.ValidateBackupCode(Email, PlayerId, CreateAnswer(backupCode, ChallengeId));

            Assert.Equal(TwoFactorValidationStatus.Valid, status);
        }

        [Fact]
        public void ValidateBackupCode_NullAnswer_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.ValidateBackupCode(Email, PlayerId, null));
        }

        [Fact]
        public void SendCode_NullPlayer_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.SendCode(null, ChallengeId, SpanishCulture));
        }

        [Fact]
        public void SendCode_KnownPlayer_SendsTheCodeToTheEmailOfThePlayer()
        {
            var player = new Player { Email = Email, DisplayName = DisplayName };

            _service.SendCode(player, ChallengeId, SpanishCulture);

            Assert.Equal(Email, _emailService.LastMessage.ToAddress);
        }

        [Fact]
        public void SendCode_FailingEmailService_ReturnsFalse()
        {
            var service = new TwoFactorCodeService(_backupCodes, new InMemoryFailingEmailService());
            var player = new Player { Email = Email, DisplayName = DisplayName };

            bool isSent = service.SendCode(player, ChallengeId, SpanishCulture);

            Assert.False(isSent);
        }

        private static VerificationCode CreateAnswer(string code, string challengeId)
        {
            return new VerificationCode { Code = code, ChallengeId = challengeId };
        }

        private void ReplaceWithDifferentCode(string oldCode)
        {
            string newCode = _service.GenerateCode(Email, ChallengeId);

            while (newCode == oldCode)
            {
                newCode = _service.GenerateCode(Email, ChallengeId);
            }
        }

        private void FailTimes(int failures)
        {
            FailTimesFor(Email, failures);
        }

        private void FailTimesFor(string email, int failures)
        {
            for (int attempt = 0; attempt < failures; attempt++)
            {
                _service.ValidateCode(email, CreateAnswer(WrongCode, ChallengeId));
            }
        }
    }
}
