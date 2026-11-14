using System;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Moderation;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Players
{
    /// <summary>
    /// Rules to change the username of a player: it must be valid, polite and not used by another player.
    /// </summary>
    public class ProfileServiceTests
    {
        private const string CurrentUsername = "emilio";
        private const string OtherUsername = "gabriel";
        private const string NewUsername = "emilio_23";
        private const string UnknownName = "nobody";
        private const string BlankName = "  ";
        private const string PaddedNewUsername = "  emilio_23  ";
        private const string InvalidUsername = "emilio 23!";
        private const string OffensiveWord = "badword";
        private const string UpperCaseCurrentUsername = "EMILIO";
        private const int CurrentPlayerId = 1;
        private const int OtherPlayerId = 2;

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly ProfileService _service;

        /// <summary>
        /// Prepares the service with two registered players.
        /// </summary>
        public ProfileServiceTests()
        {
            _service = new ProfileService(_players, new OffensiveWordFilter(new[] { OffensiveWord }));
            _players.Add(CreatePlayer(CurrentPlayerId, CurrentUsername));
            _players.Add(CreatePlayer(OtherPlayerId, OtherUsername));
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProfileService(null, new OffensiveWordFilter(new string[0])));
        }

        [Fact]
        public void Constructor_NullWordFilter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProfileService(_players, null));
        }

        [Fact]
        public void ChangeUsername_FreeValidName_ReturnsSuccess()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, NewUsername);

            Assert.Equal(UsernameChangeStatus.Success, status);
        }

        [Fact]
        public void ChangeUsername_FreeValidName_SavesTheNewUsername()
        {
            _service.ChangeUsername(CurrentUsername, NewUsername);

            Assert.Equal(NewUsername, _players.Players[0].Username);
        }

        [Fact]
        public void ChangeUsername_FreeValidName_UpdatesTheDisplayName()
        {
            _service.ChangeUsername(CurrentUsername, NewUsername);

            Assert.Equal(NewUsername, _players.Players[0].DisplayName);
        }

        [Fact]
        public void ChangeUsername_NameWithSpaces_SavesItWithoutThem()
        {
            _service.ChangeUsername(CurrentUsername, PaddedNewUsername);

            Assert.Equal(NewUsername, _players.Players[0].Username);
        }

        [Fact]
        public void ChangeUsername_UnknownPlayer_ReturnsPlayerNotFound()
        {
            UsernameChangeStatus status = _service.ChangeUsername(UnknownName, NewUsername);

            Assert.Equal(UsernameChangeStatus.PlayerNotFound, status);
        }

        [Fact]
        public void ChangeUsername_BlankCurrentName_ReturnsPlayerNotFound()
        {
            UsernameChangeStatus status = _service.ChangeUsername(BlankName, NewUsername);

            Assert.Equal(UsernameChangeStatus.PlayerNotFound, status);
        }

        [Fact]
        public void ChangeUsername_InvalidCharacters_ReturnsInvalidUsername()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, InvalidUsername);

            Assert.Equal(UsernameChangeStatus.InvalidUsername, status);
        }

        [Fact]
        public void ChangeUsername_NullNewUsername_ReturnsInvalidUsername()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, null);

            Assert.Equal(UsernameChangeStatus.InvalidUsername, status);
        }

        [Fact]
        public void ChangeUsername_OffensiveWord_ReturnsOffensiveUsername()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, OffensiveWord);

            Assert.Equal(UsernameChangeStatus.OffensiveUsername, status);
        }

        [Fact]
        public void ChangeUsername_UsedByAnotherPlayer_ReturnsUsernameAlreadyTaken()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, OtherUsername);

            Assert.Equal(UsernameChangeStatus.UsernameAlreadyTaken, status);
        }

        [Fact]
        public void ChangeUsername_UsedByAnotherPlayer_KeepsTheCurrentUsername()
        {
            _service.ChangeUsername(CurrentUsername, OtherUsername);

            Assert.Equal(CurrentUsername, _players.Players[0].Username);
        }

        [Fact]
        public void ChangeUsername_SameNameWithOtherCase_ReturnsSuccess()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, UpperCaseCurrentUsername);

            Assert.Equal(UsernameChangeStatus.Success, status);
        }

        [Fact]
        public void ChangeUsername_SameName_ReturnsSuccess()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, CurrentUsername);

            Assert.Equal(UsernameChangeStatus.Success, status);
        }

        private static Player CreatePlayer(int id, string username)
        {
            var player = new Player
            {
                Id = id,
                Username = username,
                DisplayName = username
            };

            return player;
        }
    }
}
