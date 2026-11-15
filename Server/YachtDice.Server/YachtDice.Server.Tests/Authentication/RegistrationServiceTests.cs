using System;
using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Moderation;
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
        private const string WhitespaceOnly = "   ";
        private const string UsernameWithSpaces = "gabriel hm";
        private const string WeakPassword = "abc";
        private const string OffensiveWord = "pendejo";
        private const string OffensiveUsername = "el_pendejo99";
        private const string EmailDomain = "@example.com";
        private const char FillCharacter = 'a';
        private const int UsernameMaxLength = 20;
        private const int EmailMaxLength = 100;
        private const int NameMaxLength = 50;
        private const int FiftyHours = 50;
        private const int TenHours = 10;
        private const int OneAccount = 1;

        private static readonly string UsernameAtTheLimit = new string(FillCharacter, UsernameMaxLength);
        private static readonly string TooLongUsername = new string(FillCharacter, UsernameMaxLength + 1);
        private static readonly string TooLongName = new string(FillCharacter, NameMaxLength + 1);
        private static readonly string TooLongEmail =
            new string(FillCharacter, EmailMaxLength - EmailDomain.Length + 1) + EmailDomain;

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly RegistrationService _service;

        /// <summary>
        /// Prepares the service with an empty list of players.
        /// </summary>
        public RegistrationServiceTests()
        {
            _service = new RegistrationService(_players, new OffensiveWordFilter(new[] { OffensiveWord }));
        }

        [Fact]
        public void RegisterPlayer_ValidData_ReturnsSuccess()
        {
            Player newPlayer = CreatePlayer(Username, Email);

            RegistrationResult registrationResult = _service.RegisterPlayer(newPlayer, Password);

            Assert.Equal(RegistrationResult.Success, registrationResult);
        }

        [Fact]
        public void RegisterPlayer_ValidData_SavesTheAccountAsUnverified()
        {
            _service.RegisterPlayer(CreatePlayer(Username, Email), Password);

            Assert.Equal(PlayerState.Unverified.ToString(), _players.Players.Single().State);
        }

        [Fact]
        public void RemoveUnverifiedAccounts_UnverifiedForFiftyHours_DeletesTheAccount()
        {
            _service.RegisterPlayer(CreatePlayer(Username, Email), Password);

            int removedCount = _service.RemoveUnverifiedAccounts(DateTime.UtcNow.AddHours(FiftyHours));

            Assert.Equal(OneAccount, removedCount);
        }

        [Fact]
        public void RemoveUnverifiedAccounts_UnverifiedForTenHours_KeepsTheAccount()
        {
            _service.RegisterPlayer(CreatePlayer(Username, Email), Password);

            _service.RemoveUnverifiedAccounts(DateTime.UtcNow.AddHours(TenHours));

            Assert.Single(_players.Players);
        }

        [Fact]
        public void RemoveUnverifiedAccounts_VerifiedOldAccount_KeepsTheAccount()
        {
            _service.RegisterPlayer(CreatePlayer(Username, Email), Password);
            _players.Players.Single().State = PlayerState.Active.ToString();

            _service.RemoveUnverifiedAccounts(DateTime.UtcNow.AddHours(FiftyHours));

            Assert.Single(_players.Players);
        }

        [Fact]
        public void RegisterPlayer_ValidData_SavesThePlayerWithTheHashedPassword()
        {
            Player newPlayer = CreatePlayer(Username, Email);
            var savedPasswordExpected = new { IsPlainText = false, MatchesThePassword = true };

            _service.RegisterPlayer(newPlayer, Password);

            Player savedPlayer = _players.Players.Single();
            var savedPasswordResult = new
            {
                IsPlainText = savedPlayer.PwdHash == Password,
                MatchesThePassword = PasswordHasher.Verify(Password, savedPlayer.PwdHash)
            };
            Assert.Equal(savedPasswordExpected, savedPasswordResult);
        }

        [Fact]
        public void RegisterPlayer_EmailAlreadyRegistered_ReturnsEmailAlreadyRegistered()
        {
            _players.Add(CreatePlayer(OtherUsername, Email));

            RegistrationResult registrationResult = _service.RegisterPlayer(CreatePlayer(Username, Email), Password);

            Assert.Equal(RegistrationResult.EmailAlreadyRegistered, registrationResult);
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

            RegistrationResult registrationResult = _service.RegisterPlayer(CreatePlayer(Username, Email), Password);

            Assert.Equal(RegistrationResult.UsernameAlreadyTaken, registrationResult);
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

            RegistrationResult registrationResult = _service.RegisterPlayer(newPlayer, Password);

            Assert.Equal(RegistrationResult.MissingRequiredField, registrationResult);
        }

        [Fact]
        public void RegisterPlayer_MalformedEmail_ReturnsInvalidEmail()
        {
            Player newPlayer = CreatePlayer(Username, InvalidEmail);

            RegistrationResult registrationResult = _service.RegisterPlayer(newPlayer, Password);

            Assert.Equal(RegistrationResult.InvalidEmail, registrationResult);
        }

        [Theory]
        [InlineData("", Email)]
        [InlineData(Username, InvalidEmail)]
        public void RegisterPlayer_InvalidData_SavesNothing(string username, string email)
        {
            _service.RegisterPlayer(CreatePlayer(username, email), Password);

            Assert.Empty(_players.Players);
        }

        [Fact]
        public void RegisterPlayer_UsernameAtTheLimit_ReturnsSuccess()
        {
            RegistrationResult registrationResult =
                _service.RegisterPlayer(CreatePlayer(UsernameAtTheLimit, Email), Password);

            Assert.Equal(RegistrationResult.Success, registrationResult);
        }

        [Fact]
        public void RegisterPlayer_UsernameLongerThanTheLimit_ReturnsFieldTooLong()
        {
            RegistrationResult registrationResult =
                _service.RegisterPlayer(CreatePlayer(TooLongUsername, Email), Password);

            Assert.Equal(RegistrationResult.FieldTooLong, registrationResult);
        }

        [Fact]
        public void RegisterPlayer_EmailLongerThanTheLimit_ReturnsFieldTooLong()
        {
            RegistrationResult registrationResult =
                _service.RegisterPlayer(CreatePlayer(Username, TooLongEmail), Password);

            Assert.Equal(RegistrationResult.FieldTooLong, registrationResult);
        }

        [Fact]
        public void RegisterPlayer_FirstNameLongerThanTheLimit_ReturnsFieldTooLong()
        {
            Player newPlayer = CreatePlayer(Username, Email);
            newPlayer.FirstName = TooLongName;

            RegistrationResult registrationResult = _service.RegisterPlayer(newPlayer, Password);

            Assert.Equal(RegistrationResult.FieldTooLong, registrationResult);
        }

        [Theory]
        [InlineData("Gabriel2")]
        [InlineData("<script>")]
        [InlineData("G")]
        public void RegisterPlayer_FirstNameWithDigitsSymbolsOrTooShort_ReturnsInvalidName(string firstName)
        {
            Player newPlayer = CreatePlayer(Username, Email);
            newPlayer.FirstName = firstName;

            RegistrationResult registrationResult = _service.RegisterPlayer(newPlayer, Password);

            Assert.Equal(RegistrationResult.InvalidName, registrationResult);
        }

        [Fact]
        public void RegisterPlayer_LastNameWithDigits_ReturnsInvalidName()
        {
            Player newPlayer = CreatePlayer(Username, Email);
            newPlayer.LastName = "Hernandez3";

            RegistrationResult registrationResult = _service.RegisterPlayer(newPlayer, Password);

            Assert.Equal(RegistrationResult.InvalidName, registrationResult);
        }

        [Fact]
        public void RegisterPlayer_NamesWithAccentsAndSeparators_ReturnsSuccess()
        {
            Player newPlayer = CreatePlayer(Username, Email);
            newPlayer.FirstName = "José María";
            newPlayer.LastName = "O'Brien-Núñez";

            RegistrationResult registrationResult = _service.RegisterPlayer(newPlayer, Password);

            Assert.Equal(RegistrationResult.Success, registrationResult);
        }

        [Fact]
        public void RegisterPlayer_InvalidName_SavesNothing()
        {
            Player newPlayer = CreatePlayer(Username, Email);
            newPlayer.FirstName = "Gabriel2";

            _service.RegisterPlayer(newPlayer, Password);

            Assert.Empty(_players.Players);
        }

        [Fact]
        public void RegisterPlayer_FieldLongerThanTheLimit_SavesNothing()
        {
            _service.RegisterPlayer(CreatePlayer(TooLongUsername, Email), Password);

            Assert.Empty(_players.Players);
        }

        [Fact]
        public void RegisterPlayer_UsernameWithSpaces_ReturnsInvalidUsername()
        {
            RegistrationResult registrationResult =
                _service.RegisterPlayer(CreatePlayer(UsernameWithSpaces, Email), Password);

            Assert.Equal(RegistrationResult.InvalidUsername, registrationResult);
        }

        [Fact]
        public void RegisterPlayer_UsernameWithOffensiveWord_ReturnsOffensiveUsername()
        {
            RegistrationResult registrationResult =
                _service.RegisterPlayer(CreatePlayer(OffensiveUsername, Email), Password);

            Assert.Equal(RegistrationResult.OffensiveUsername, registrationResult);
        }

        [Fact]
        public void RegisterPlayer_WeakPassword_ReturnsWeakPassword()
        {
            RegistrationResult registrationResult =
                _service.RegisterPlayer(CreatePlayer(Username, Email), WeakPassword);

            Assert.Equal(RegistrationResult.WeakPassword, registrationResult);
        }

        [Fact]
        public void RegisterPlayer_WeakPassword_SavesNothing()
        {
            _service.RegisterPlayer(CreatePlayer(Username, Email), WeakPassword);

            Assert.Empty(_players.Players);
        }

        [Fact]
        public void RegisterPlayer_LastNameWithOnlySpaces_ReturnsMissingRequiredField()
        {
            Player newPlayer = CreatePlayer(Username, Email);
            newPlayer.LastName = WhitespaceOnly;

            RegistrationResult registrationResult = _service.RegisterPlayer(newPlayer, Password);

            Assert.Equal(RegistrationResult.MissingRequiredField, registrationResult);
        }

        [Fact]
        public void RegisterPlayer_PasswordWithOnlySpaces_ReturnsMissingRequiredField()
        {
            RegistrationResult registrationResult =
                _service.RegisterPlayer(CreatePlayer(Username, Email), WhitespaceOnly);

            Assert.Equal(RegistrationResult.MissingRequiredField, registrationResult);
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
