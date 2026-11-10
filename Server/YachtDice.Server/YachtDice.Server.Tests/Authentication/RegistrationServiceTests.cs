using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Authentication
{
    /// <summary>
    /// Basic rules of the registration of a new player.
    /// </summary>
    public class RegistrationServiceTests
    {
        private const string Username = "gabrielhernandez";
        private const string OtherUsername = "emilio23";
        private const string Email = "gabriel@example.com";
        private const string OtherEmail = "emilio@example.com";
        private const string InvalidEmail = "not-an-email";
        private const string Password = "Secret123!";
        private const string FirstName = "Gabriel";
        private const string LastName = "Hernandez";

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly RegistrationService _service;

        /// <summary>
        /// Prepares the service with an empty list of players.
        /// </summary>
        public RegistrationServiceTests()
        {
            _service = new RegistrationService(_players);
        }

        [Fact]
        public void RegisterPlayer_ValidData_ReturnsSuccess()
        {
            Player newPlayer = CreatePlayer(Username, Email);

            RegistrationResult result = _service.RegisterPlayer(newPlayer, Password);

            Assert.Equal(RegistrationResult.Success, result);
        }

        [Fact]
        public void RegisterPlayer_ValidData_SavesThePlayerWithTheHashedPassword()
        {
            Player newPlayer = CreatePlayer(Username, Email);
            var expected = new { IsPlainText = false, MatchesThePassword = true };

            _service.RegisterPlayer(newPlayer, Password);

            Player savedPlayer = _players.Players.Single();
            var actual = new
            {
                IsPlainText = savedPlayer.PwdHash == Password,
                MatchesThePassword = PasswordHasher.Verify(Password, savedPlayer.PwdHash)
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RegisterPlayer_EmailAlreadyRegistered_ReturnsEmailAlreadyRegistered()
        {
            _players.Add(CreatePlayer(OtherUsername, Email));

            RegistrationResult result = _service.RegisterPlayer(CreatePlayer(Username, Email), Password);

            Assert.Equal(RegistrationResult.EmailAlreadyRegistered, result);
        }

        [Fact]
        public void RegisterPlayer_EmailAlreadyRegistered_KeepsOnlyTheFirstPlayer()
        {
            _players.Add(CreatePlayer(OtherUsername, Email));

            _service.RegisterPlayer(CreatePlayer(Username, Email), Password);

            Assert.Single(_players.Players);
        }

        [Fact]
        public void RegisterPlayer_UsernameAlreadyTaken_ReturnsUsernameAlreadyTaken()
        {
            _players.Add(CreatePlayer(Username, OtherEmail));

            RegistrationResult result = _service.RegisterPlayer(CreatePlayer(Username, Email), Password);

            Assert.Equal(RegistrationResult.UsernameAlreadyTaken, result);
        }

        [Fact]
        public void RegisterPlayer_UsernameAlreadyTaken_KeepsOnlyTheFirstPlayer()
        {
            _players.Add(CreatePlayer(Username, OtherEmail));

            _service.RegisterPlayer(CreatePlayer(Username, Email), Password);

            Assert.Single(_players.Players);
        }

        [Fact]
        public void RegisterPlayer_BlankUsername_ReturnsMissingRequiredField()
        {
            Player newPlayer = CreatePlayer(string.Empty, Email);

            RegistrationResult result = _service.RegisterPlayer(newPlayer, Password);

            Assert.Equal(RegistrationResult.MissingRequiredField, result);
        }

        [Fact]
        public void RegisterPlayer_MalformedEmail_ReturnsInvalidEmail()
        {
            Player newPlayer = CreatePlayer(Username, InvalidEmail);

            RegistrationResult result = _service.RegisterPlayer(newPlayer, Password);

            Assert.Equal(RegistrationResult.InvalidEmail, result);
        }

        [Theory]
        [InlineData("", Email)]
        [InlineData(Username, InvalidEmail)]
        public void RegisterPlayer_InvalidData_SavesNothing(string username, string email)
        {
            _service.RegisterPlayer(CreatePlayer(username, email), Password);

            Assert.Empty(_players.Players);
        }

        private static Player CreatePlayer(string username, string email)
        {
            var player = new Player
            {
                Username = username,
                Email = email,
                FirstName = FirstName,
                LastName = LastName
            };

            return player;
        }
    }
}
