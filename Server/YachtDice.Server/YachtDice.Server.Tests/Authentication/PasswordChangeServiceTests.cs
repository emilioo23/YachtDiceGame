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
        private readonly PasswordChangeService _service;

        /// <summary>
        /// Prepares the service with one registered player.
        /// </summary>
        public PasswordChangeServiceTests()
        {
            _players.Add(new Player
            {
                Id = PlayerId,
                DisplayName = DisplayName,
                PwdHash = PasswordHasher.ComputeHash(OldPassword)
            });
            _passwords = new InMemoryPasswordResetRepository(_players);
            _service = new PasswordChangeService(_players, _passwords);
        }

        [Fact]
        public void ChangePassword_RightCurrentPassword_ReturnsSuccess()
        {
            PasswordChangeStatus status = _service.ChangePassword(DisplayName, CreateRequest(OldPassword, NewPassword));

            Assert.Equal(PasswordChangeStatus.Success, status);
        }

        [Fact]
        public void ChangePassword_RightCurrentPassword_SavesTheNewPassword()
        {
            _service.ChangePassword(DisplayName, CreateRequest(OldPassword, NewPassword));

            Assert.True(PasswordHasher.Verify(NewPassword, _players.Players.Single().PwdHash));
        }

        [Fact]
        public void ChangePassword_WrongCurrentPassword_ReturnsWrongCurrentPassword()
        {
            PasswordChangeStatus status = _service.ChangePassword(
                DisplayName,
                CreateRequest(WrongPassword, NewPassword));

            Assert.Equal(PasswordChangeStatus.WrongCurrentPassword, status);
        }

        [Fact]
        public void ChangePassword_WrongCurrentPassword_KeepsTheOldPassword()
        {
            _service.ChangePassword(DisplayName, CreateRequest(WrongPassword, NewPassword));

            Assert.True(PasswordHasher.Verify(OldPassword, _players.Players.Single().PwdHash));
        }

        [Fact]
        public void ChangePassword_EmptyCurrentPassword_ReturnsWrongCurrentPassword()
        {
            PasswordChangeStatus status = _service.ChangePassword(
                DisplayName,
                CreateRequest(string.Empty, NewPassword));

            Assert.Equal(PasswordChangeStatus.WrongCurrentPassword, status);
        }

        [Fact]
        public void ChangePassword_CurrentPasswordLongerThanTheMaximum_ReturnsWrongCurrentPassword()
        {
            string tooLong = new string(FillCharacter, CredentialRules.PasswordMaxLength + 1);

            PasswordChangeStatus status = _service.ChangePassword(DisplayName, CreateRequest(tooLong, NewPassword));

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
                CreateRequest(OldPassword, weakPassword));

            Assert.Equal(PasswordChangeStatus.InvalidNewPassword, status);
        }

        [Fact]
        public void ChangePassword_NewPasswordLongerThanTheMaximum_ReturnsInvalidNewPassword()
        {
            string tooLong = new string(FillCharacter, CredentialRules.PasswordMaxLength) + Digit;

            PasswordChangeStatus status = _service.ChangePassword(DisplayName, CreateRequest(OldPassword, tooLong));

            Assert.Equal(PasswordChangeStatus.InvalidNewPassword, status);
        }

        [Fact]
        public void ChangePassword_WeakNewPassword_KeepsTheOldPassword()
        {
            _service.ChangePassword(DisplayName, CreateRequest(OldPassword, WeakPassword));

            Assert.True(PasswordHasher.Verify(OldPassword, _players.Players.Single().PwdHash));
        }

        [Fact]
        public void ChangePassword_NewPasswordEqualToTheCurrentOne_ReturnsPasswordUsedBefore()
        {
            PasswordChangeStatus status = _service.ChangePassword(
                DisplayName,
                CreateRequest(OldPassword, OldPassword));

            Assert.Equal(PasswordChangeStatus.PasswordUsedBefore, status);
        }

        [Fact]
        public void ChangePassword_ReturningToThePreviousPassword_ReturnsPasswordUsedBefore()
        {
            _service.ChangePassword(DisplayName, CreateRequest(OldPassword, NewPassword));

            PasswordChangeStatus status = _service.ChangePassword(
                DisplayName,
                CreateRequest(NewPassword, OldPassword));

            Assert.Equal(PasswordChangeStatus.PasswordUsedBefore, status);
        }

        [Fact]
        public void ChangePassword_UnknownPlayer_ReturnsPlayerNotFound()
        {
            PasswordChangeStatus status = _service.ChangePassword(
                UnknownName,
                CreateRequest(OldPassword, NewPassword));

            Assert.Equal(PasswordChangeStatus.PlayerNotFound, status);
        }

        [Fact]
        public void ChangePassword_BlankName_ReturnsPlayerNotFound()
        {
            PasswordChangeStatus status = _service.ChangePassword(BlankName, CreateRequest(OldPassword, NewPassword));

            Assert.Equal(PasswordChangeStatus.PlayerNotFound, status);
        }

        [Fact]
        public void ChangePassword_TooManyWrongCurrentPasswords_LocksTheAccount()
        {
            RegisterWrongPasswords(LoginAttemptTracker.FailuresForTemporaryLock);

            PasswordChangeStatus status = _service.ChangePassword(
                DisplayName,
                CreateRequest(OldPassword, NewPassword));

            Assert.Equal(PasswordChangeStatus.TooManyAttempts, status);
        }

        [Fact]
        public void ChangePassword_LockedAccount_KeepsTheOldPassword()
        {
            RegisterWrongPasswords(LoginAttemptTracker.FailuresForTemporaryLock);

            _service.ChangePassword(DisplayName, CreateRequest(OldPassword, NewPassword));

            Assert.True(PasswordHasher.Verify(OldPassword, _players.Players.Single().PwdHash));
        }

        [Fact]
        public void ChangePassword_NullRequest_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.ChangePassword(DisplayName, null));
        }

        [Fact]
        public void Constructor_NullPlayerRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PasswordChangeService(null, _passwords));
        }

        [Fact]
        public void Constructor_NullPasswordRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PasswordChangeService(_players, null));
        }

        private static PasswordChangeRequest CreateRequest(string currentPassword, string newPassword)
        {
            return new PasswordChangeRequest { CurrentPassword = currentPassword, NewPassword = newPassword };
        }

        private void RegisterWrongPasswords(int count)
        {
            for (int attempt = 0; attempt < count; attempt++)
            {
                _service.ChangePassword(DisplayName, CreateRequest(WrongPassword, NewPassword));
            }
        }
    }
}
