using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Logic.TwoFactor;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.TwoFactor
{
    /// <summary>
    /// Backup codes: the player creates them with the password while the two-step verification is on, each one
    /// replaces the code sent by email only once, and they are deleted when the verification is turned off.
    /// </summary>
    public class BackupCodeTests
    {
        private const int PlayerId = 1;
        private const string Email = "gabriel@gmail.com";
        private const string DisplayName = "gabrielhernandez";
        private const string UnknownDisplayName = "carlosramirez";
        private const string Password = "Secret123!";
        private const string WrongPassword = "Wrong123!";
        private const string ChallengeId = "challenge-1";
        private const string SecondChallengeId = "challenge-2";
        private const string WrongBackupCode = "00000000";
        private const int CodesLeftAfterOneUse = 9;
        private const int NoCodes = 0;

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryBackupCodeRepository _backupCodes = new InMemoryBackupCodeRepository();
        private readonly TwoFactorCodeService _codeService;
        private readonly TwoFactorVerificationService _verificationService;
        private readonly TwoFactorSettingsService _settingsService;

        /// <summary>
        /// Prepares the services with one registered player who has the two-step verification turned on.
        /// </summary>
        public BackupCodeTests()
        {
            _codeService = new TwoFactorCodeService(_backupCodes);
            var deliveryService = new TwoFactorDeliveryService(_codeService, new InMemoryEmailService());

            _players.Add(new Player
            {
                Id = PlayerId,
                Email = Email,
                PwdHash = PasswordHasher.ComputeHash(Password),
                DisplayName = DisplayName
            });
            _players.SaveTwoFactorEnabled(PlayerId, true);
            _verificationService = new TwoFactorVerificationService(_players, _codeService, deliveryService);
            _settingsService = new TwoFactorSettingsService(_players, _verificationService, deliveryService);
        }

        [Fact]
        public void CreateBackupCodes_TwoFactorEnabled_ReturnsTheCodesOfTheRightLength()
        {
            var creationExpected = new
            {
                Status = BackupCodeCreationStatus.Created,
                CodeCount = TwoFactorCodeService.BackupCodeCount,
                AreBackupCodes = true
            };

            BackupCodeCreation creation = _settingsService.CreateBackupCodes(DisplayName, Password);

            var creationResult = new
            {
                creation.Status,
                CodeCount = creation.Codes.Count,
                AreBackupCodes = creation.Codes.All(TwoFactorCodeService.IsBackupCodeFormat)
            };
            Assert.Equal(creationExpected, creationResult);
        }

        [Fact]
        public void CreateBackupCodes_Created_SavesOnlyTheHashes()
        {
            BackupCodeCreation creation = _settingsService.CreateBackupCodes(DisplayName, Password);

            List<string> savedCodes = _backupCodes.Codes.Select(backupCode => backupCode.Code).ToList();
            bool hasPlainCode = creation.Codes.Intersect(savedCodes).Any();
            Assert.False(hasPlainCode);
        }

        [Fact]
        public void CreateBackupCodes_WrongPassword_ReturnsInvalidPassword()
        {
            BackupCodeCreation creation = _settingsService.CreateBackupCodes(DisplayName, WrongPassword);

            Assert.Equal(BackupCodeCreationStatus.InvalidPassword, creation.Status);
        }

        [Fact]
        public void CreateBackupCodes_TwoFactorDisabled_ReturnsTwoFactorDisabled()
        {
            _players.SaveTwoFactorEnabled(PlayerId, false);

            BackupCodeCreation creation = _settingsService.CreateBackupCodes(DisplayName, Password);

            Assert.Equal(BackupCodeCreationStatus.TwoFactorDisabled, creation.Status);
        }

        [Fact]
        public void CreateBackupCodes_UnknownPlayer_ReturnsPlayerNotFound()
        {
            BackupCodeCreation creation = _settingsService.CreateBackupCodes(UnknownDisplayName, Password);

            Assert.Equal(BackupCodeCreationStatus.PlayerNotFound, creation.Status);
        }

        [Fact]
        public void VerifyPlayerCode_ActiveBackupCode_ReturnsValid()
        {
            string backupCode = _settingsService.CreateBackupCodes(DisplayName, Password).Codes.First();
            _codeService.GenerateCode(Email, ChallengeId);

            TwoFactorValidationStatus status = _verificationService.VerifyPlayerCode(
                Email,
                new VerificationCode { Code = backupCode, ChallengeId = ChallengeId });

            Assert.Equal(TwoFactorValidationStatus.Valid, status);
        }

        [Fact]
        public void VerifyPlayerCode_BackupCodeUsedBefore_ReturnsIncorrect()
        {
            string backupCode = _settingsService.CreateBackupCodes(DisplayName, Password).Codes.First();
            UseBackupCode(backupCode, ChallengeId);
            _codeService.GenerateCode(Email, SecondChallengeId);

            TwoFactorValidationStatus status = _verificationService.VerifyPlayerCode(
                Email,
                new VerificationCode { Code = backupCode, ChallengeId = SecondChallengeId });

            Assert.Equal(TwoFactorValidationStatus.Incorrect, status);
        }

        [Fact]
        public void VerifyPlayerCode_WrongBackupCode_ReturnsIncorrect()
        {
            _settingsService.CreateBackupCodes(DisplayName, Password);
            _codeService.GenerateCode(Email, ChallengeId);

            TwoFactorValidationStatus status = _verificationService.VerifyPlayerCode(
                Email,
                new VerificationCode { Code = WrongBackupCode, ChallengeId = ChallengeId });

            Assert.Equal(TwoFactorValidationStatus.Incorrect, status);
        }

        [Fact]
        public void VerifyPlayerCode_BackupCodeWithoutAnOpenChallenge_ReturnsExpired()
        {
            string backupCode = _settingsService.CreateBackupCodes(DisplayName, Password).Codes.First();

            TwoFactorValidationStatus status = _verificationService.VerifyPlayerCode(
                Email,
                new VerificationCode { Code = backupCode, ChallengeId = ChallengeId });

            Assert.Equal(TwoFactorValidationStatus.Expired, status);
        }

        [Fact]
        public void CountBackupCodes_AfterUsingOne_ReturnsTheCodesLeft()
        {
            string backupCode = _settingsService.CreateBackupCodes(DisplayName, Password).Codes.First();
            UseBackupCode(backupCode, ChallengeId);

            int codeCount = _settingsService.CountBackupCodes(DisplayName);

            Assert.Equal(CodesLeftAfterOneUse, codeCount);
        }

        [Fact]
        public void Disable_PlayerWithBackupCodes_DeletesTheCodes()
        {
            _settingsService.CreateBackupCodes(DisplayName, Password);

            _settingsService.Disable(DisplayName, Password);

            Assert.Equal(NoCodes, _backupCodes.Codes.Count);
        }

        private void UseBackupCode(string backupCode, string challengeId)
        {
            _codeService.GenerateCode(Email, challengeId);
            _verificationService.VerifyPlayerCode(
                Email,
                new VerificationCode { Code = backupCode, ChallengeId = challengeId });
        }
    }
}
