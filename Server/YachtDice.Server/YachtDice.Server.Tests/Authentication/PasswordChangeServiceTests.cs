using System;
using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Authentication
{
    /// <summary>
    /// Rules of the password change that a player asks for from the settings while being logged in.
    /// </summary>
    public class PasswordChangeServiceTests
    {
        private const int PlayerId = 7;
        private const char FillCharacter = 'a';
        private const string DisplayName = "emilio";
        private const string UnknownName = "nobody";
        private const string BlankName = "  ";
        private const string OldPassword = "OldPass123!";
        private const string NewPassword = "NewPass456!";
        private const string WrongPassword = "WrongPass789!";
        private const string WeakPassword = "abc";
        private const string Digit = "1";

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryPasswordResetRepository _passwords;
        private readonly PasswordService _service;

        /// <summary>
        /// Prepares the service with one registered player.
        /// </summary>
        public PasswordChangeServiceTests()
        {
            _players.Add(new Player
            {
                Id = PlayerId,
                DisplayName = DisplayName,
                PwdHash = CredentialRules.ComputeHash(OldPassword)
            });
            _passwords = new InMemoryPasswordResetRepository(_players);
            _service = new PasswordService(_players, _passwords, new InMemoryEmailService());
        }

        [Fact]
        public void ChangePassword_RightCurrentPassword_ReturnsSuccess()
        {
            PasswordChangeStatus status = _service.ChangePassword(DisplayName, OldPassword, NewPassword);

            Assert.Equal(PasswordChangeStatus.Success, status);
        }

        [Fact]
        public void ChangePassword_RightCurrentPassword_SavesTheNewPassword()
        {
            _service.ChangePassword(DisplayName, OldPassword, NewPassword);

            Assert.True(CredentialRules.Verify(NewPassword, _players.Players.Single().PwdHash));
        }

        [Fact]
        public void ChangePassword_WrongCurrentPassword_ReturnsWrongCurrentPassword()
        {
            PasswordChangeStatus status = _service.ChangePassword(
                DisplayName,
                WrongPassword, NewPassword);

            Assert.Equal(PasswordChangeStatus.WrongCurrentPassword, status);
        }

        [Fact]
        public void ChangePassword_WrongCurrentPassword_KeepsTheOldPassword()
        {
            _service.ChangePassword(DisplayName, WrongPassword, NewPassword);

            Assert.True(CredentialRules.Verify(OldPassword, _players.Players.Single().PwdHash));
        }

        [Fact]
        public void ChangePassword_EmptyCurrentPassword_ReturnsWrongCurrentPassword()
        {
            PasswordChangeStatus status = _service.ChangePassword(
                DisplayName,
                string.Empty, NewPassword);

            Assert.Equal(PasswordChangeStatus.WrongCurrentPassword, status);
        }

        [Fact]
        public void ChangePassword_CurrentPasswordLongerThanTheMaximum_ReturnsWrongCurrentPassword()
        {
            string tooLong = new string(FillCharacter, CredentialRules.PasswordMaxLength + 1);

            PasswordChangeStatus status = _service.ChangePassword(DisplayName, tooLong, NewPassword);

            Assert.Equal(PasswordChangeStatus.WrongCurrentPassword, status);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12345678")]
        [InlineData("abcdefgh")]
        public void ChangePassword_WeakNewPassword_ReturnsInvalidNewPassword(string weakPassword)
        {
            PasswordChangeStatus status = _service.ChangePassword(
                DisplayName,
                OldPassword, weakPassword);

            Assert.Equal(PasswordChangeStatus.InvalidNewPassword, status);
        }

        [Fact]
        public void ChangePassword_NewPasswordLongerThanTheMaximum_ReturnsInvalidNewPassword()
        {
            string tooLong = new string(FillCharacter, CredentialRules.PasswordMaxLength) + Digit;

            PasswordChangeStatus status = _service.ChangePassword(DisplayName, OldPassword, tooLong);

            Assert.Equal(PasswordChangeStatus.InvalidNewPassword, status);
        }

        [Fact]
        public void ChangePassword_WeakNewPassword_KeepsTheOldPassword()
        {
            _service.ChangePassword(DisplayName, OldPassword, WeakPassword);

            Assert.True(CredentialRules.Verify(OldPassword, _players.Players.Single().PwdHash));
        }

        [Fact]
        public void ChangePassword_NewPasswordEqualToTheCurrentOne_ReturnsPasswordUsedBefore()
        {
            PasswordChangeStatus status = _service.ChangePassword(
                DisplayName,
                OldPassword, OldPassword);

            Assert.Equal(PasswordChangeStatus.PasswordUsedBefore, status);
        }

        [Fact]
        public void ChangePassword_ReturningToThePreviousPassword_ReturnsPasswordUsedBefore()
        {
            _service.ChangePassword(DisplayName, OldPassword, NewPassword);

            PasswordChangeStatus status = _service.ChangePassword(
                DisplayName,
                NewPassword, OldPassword);

            Assert.Equal(PasswordChangeStatus.PasswordUsedBefore, status);
        }

        [Fact]
        public void ChangePassword_UnknownPlayer_ReturnsPlayerNotFound()
        {
            PasswordChangeStatus status = _service.ChangePassword(
                UnknownName,
                OldPassword, NewPassword);

            Assert.Equal(PasswordChangeStatus.PlayerNotFound, status);
        }

        [Fact]
        public void ChangePassword_BlankName_ReturnsPlayerNotFound()
        {
            PasswordChangeStatus status = _service.ChangePassword(BlankName, OldPassword, NewPassword);

            Assert.Equal(PasswordChangeStatus.PlayerNotFound, status);
        }

        [Fact]
        public void ChangePassword_TooManyWrongCurrentPasswords_LocksTheAccount()
        {
            RegisterWrongPasswords(LoginAttemptTracker.FailuresForTemporaryLock);

            PasswordChangeStatus status = _service.ChangePassword(
                DisplayName,
                OldPassword, NewPassword);

            Assert.Equal(PasswordChangeStatus.TooManyAttempts, status);
        }

        [Fact]
        public void ChangePassword_LockedAccount_KeepsTheOldPassword()
        {
            RegisterWrongPasswords(LoginAttemptTracker.FailuresForTemporaryLock);

            _service.ChangePassword(DisplayName, OldPassword, NewPassword);

            Assert.True(CredentialRules.Verify(OldPassword, _players.Players.Single().PwdHash));
        }

        [Fact]
        public void GetSecuritySummary_PlayerWhoNeverChangedThePassword_SaysNothingHappened()
        {
            var summaryExpected = new { HasPasswordChange = false, HasRecentLock = false };

            SecuritySummary summary = _service.GetSecuritySummary(DisplayName);

            Assert.Equivalent(summaryExpected, summary);
        }

        [Fact]
        public void GetSecuritySummary_RecentLock_ShowsTheLockAndItsReason()
        {
            DateTime lockedUntil = DateTime.UtcNow.AddHours(1);
            _players.SaveLock(PlayerId, LoginLock.Daily.ToString(), lockedUntil);
            var summaryExpected = new
            {
                HasRecentLock = true,
                LockedUntil = lockedUntil,
                LockReason = LoginLock.Daily.ToString()
            };

            SecuritySummary summary = _service.GetSecuritySummary(DisplayName);

            Assert.Equivalent(summaryExpected, summary);
        }

        [Fact]
        public void Constructor_NullPlayerRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new PasswordService(null, _passwords, new InMemoryEmailService()));
        }

        [Fact]
        public void Constructor_NullPasswordRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PasswordService(_players, null, new InMemoryEmailService()));
        }

        private void RegisterWrongPasswords(int count)
        {
            for (int attempt = 0; attempt < count; attempt++)
            {
                _service.ChangePassword(DisplayName, WrongPassword, NewPassword);
            }
        }
    }
}
