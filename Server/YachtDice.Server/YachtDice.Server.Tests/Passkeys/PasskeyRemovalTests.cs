using System;
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
            Func<Player, bool> isTestPlayer = player => player.Id == PasskeyTestContext.PlayerId;
            Func<PasskeyRecord, bool> isFirstCredential = record => record.CredentialId == FirstCredential;
            Func<PasskeyRecord, bool> isOtherCredential = record => record.CredentialId == OtherCredential;

            _context.Players.Players.First(isTestPlayer).PwdHash = CredentialRules.ComputeHash(Password);
            AddPasskey(PasskeyTestContext.PlayerId, FirstCredential);
            AddPasskey(PasskeyTestContext.PlayerId, SecondCredential);
            AddPasskey(PasskeyTestContext.OtherPlayerId, OtherCredential);
            _firstId = _context.Passkeys.Records.First(isFirstCredential).Id;
            _otherPlayerPasskeyId = _context.Passkeys.Records.First(isOtherCredential).Id;
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
            Func<PasskeyRecord, bool> isFirstPasskey = record => record.Id == _firstId;

            _context.Service.RemovePasskey(PasskeyTestContext.DisplayName, _firstId, Password);
            bool hasPasskey = _context.Passkeys.Records.Any(isFirstPasskey);

            Assert.False(hasPasskey);
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
            Func<PasskeyRecord, bool> isFirstPasskey = record => record.Id == _firstId;

            _context.Service.RemovePasskey(PasskeyTestContext.DisplayName, _firstId, WrongPassword);
            bool hasPasskey = _context.Passkeys.Records.Any(isFirstPasskey);

            Assert.True(hasPasskey);
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
            Func<PasskeyRecord, bool> isOtherPasskey = record => record.Id == _otherPlayerPasskeyId;

            _context.Service.RemovePasskey(PasskeyTestContext.DisplayName, _otherPlayerPasskeyId, Password);
            bool hasPasskey = _context.Passkeys.Records.Any(isOtherPasskey);

            Assert.True(hasPasskey);
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
