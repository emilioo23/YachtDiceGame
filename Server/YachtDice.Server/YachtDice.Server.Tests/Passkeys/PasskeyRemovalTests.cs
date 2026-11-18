using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Passkeys;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Rules of listing and removing the passkeys of a player.
    /// </summary>
    public class PasskeyRemovalTests
    {
        private const string Password = "Correct-Password-1";
        private const string WrongPassword = "Wrong-Password-1";
        private const string UnknownName = "nobody";
        private const string FirstCredential = "credential-one";
        private const string SecondCredential = "credential-two";
        private const string OtherCredential = "credential-other";
        private const string DeviceNamePrefix = "Passkey ";
        private const int UnknownPasskeyId = 999;
        private const int TwoPasskeys = 2;
        private const int NoPasskeys = 0;

        private readonly PasskeyTestContext _context = new PasskeyTestContext();
        private readonly int _firstId;
        private readonly int _otherPlayerPasskeyId;

        /// <summary>
        /// Prepares two passkeys for the first player and one for the second.
        /// </summary>
        public PasskeyRemovalTests()
        {
            _context.Players.Players.First(player => player.Id == PasskeyTestContext.PlayerId).PwdHash =
                PasswordHasher.ComputeHash(Password);
            AddPasskey(PasskeyTestContext.PlayerId, FirstCredential);
            AddPasskey(PasskeyTestContext.PlayerId, SecondCredential);
            AddPasskey(PasskeyTestContext.OtherPlayerId, OtherCredential);
            _firstId = _context.Passkeys.Records.First(record => record.CredentialId == FirstCredential).Id;
            _otherPlayerPasskeyId = _context.Passkeys.Records
                .First(record => record.CredentialId == OtherCredential)
                .Id;
        }

        [Fact]
        public void ListPasskeys_PlayerWithTwoPasskeys_ReturnsTwo()
        {
            int passkeyCountExpected = TwoPasskeys;

            int passkeyCountResult = _context.Service.ListPasskeys(PasskeyTestContext.DisplayName).Count;

            Assert.Equal(passkeyCountExpected, passkeyCountResult);
        }

        [Fact]
        public void ListPasskeys_UnknownPlayer_ReturnsNone()
        {
            int passkeyCountExpected = NoPasskeys;

            int passkeyCountResult = _context.Service.ListPasskeys(UnknownName).Count;

            Assert.Equal(passkeyCountExpected, passkeyCountResult);
        }

        [Fact]
        public void RemovePasskey_RightPassword_ReturnsRemoved()
        {
            PasskeyRemovalStatus removalStatusExpected = PasskeyRemovalStatus.Removed;

            PasskeyRemovalStatus removalStatusResult = _context.Service.RemovePasskey(
                PasskeyTestContext.DisplayName,
                _firstId,
                Password);

            Assert.Equal(removalStatusExpected, removalStatusResult);
        }

        [Fact]
        public void RemovePasskey_RightPassword_DeletesThePasskey()
        {
            _context.Service.RemovePasskey(PasskeyTestContext.DisplayName, _firstId, Password);
            bool passkeyExists = _context.Passkeys.Records.Any(record => record.Id == _firstId);

            Assert.False(passkeyExists);
        }

        [Fact]
        public void RemovePasskey_WrongPassword_ReturnsInvalidPassword()
        {
            PasskeyRemovalStatus removalStatusExpected = PasskeyRemovalStatus.InvalidPassword;

            PasskeyRemovalStatus removalStatusResult = _context.Service.RemovePasskey(
                PasskeyTestContext.DisplayName,
                _firstId,
                WrongPassword);

            Assert.Equal(removalStatusExpected, removalStatusResult);
        }

        [Fact]
        public void RemovePasskey_WrongPassword_KeepsThePasskey()
        {
            _context.Service.RemovePasskey(PasskeyTestContext.DisplayName, _firstId, WrongPassword);
            bool passkeyExists = _context.Passkeys.Records.Any(record => record.Id == _firstId);

            Assert.True(passkeyExists);
        }

        [Fact]
        public void RemovePasskey_EmptyPassword_ReturnsInvalidPassword()
        {
            PasskeyRemovalStatus removalStatusExpected = PasskeyRemovalStatus.InvalidPassword;

            PasskeyRemovalStatus removalStatusResult = _context.Service.RemovePasskey(
                PasskeyTestContext.DisplayName,
                _firstId,
                string.Empty);

            Assert.Equal(removalStatusExpected, removalStatusResult);
        }

        [Fact]
        public void RemovePasskey_PasskeyOfAnotherPlayer_ReturnsPasskeyNotFound()
        {
            PasskeyRemovalStatus removalStatusExpected = PasskeyRemovalStatus.PasskeyNotFound;

            PasskeyRemovalStatus removalStatusResult = _context.Service.RemovePasskey(
                PasskeyTestContext.DisplayName,
                _otherPlayerPasskeyId,
                Password);

            Assert.Equal(removalStatusExpected, removalStatusResult);
        }

        [Fact]
        public void RemovePasskey_PasskeyOfAnotherPlayer_KeepsThePasskey()
        {
            _context.Service.RemovePasskey(PasskeyTestContext.DisplayName, _otherPlayerPasskeyId, Password);
            bool passkeyExists = _context.Passkeys.Records.Any(record => record.Id == _otherPlayerPasskeyId);

            Assert.True(passkeyExists);
        }

        [Fact]
        public void RemovePasskey_UnknownIdentifier_ReturnsPasskeyNotFound()
        {
            PasskeyRemovalStatus removalStatusExpected = PasskeyRemovalStatus.PasskeyNotFound;

            PasskeyRemovalStatus removalStatusResult = _context.Service.RemovePasskey(
                PasskeyTestContext.DisplayName,
                UnknownPasskeyId,
                Password);

            Assert.Equal(removalStatusExpected, removalStatusResult);
        }

        [Fact]
        public void RemovePasskey_UnknownPlayer_ReturnsPlayerNotFound()
        {
            PasskeyRemovalStatus removalStatusExpected = PasskeyRemovalStatus.PlayerNotFound;

            PasskeyRemovalStatus removalStatusResult = _context.Service.RemovePasskey(
                UnknownName,
                _firstId,
                Password);

            Assert.Equal(removalStatusExpected, removalStatusResult);
        }

        private void AddPasskey(int playerId, string credentialId)
        {
            _context.Passkeys.Add(new PasskeyRecord
            {
                PlayerId = playerId,
                CredentialId = credentialId,
                PublicKey = credentialId,
                DeviceName = DeviceNamePrefix + credentialId
            });
        }
    }
}
