using System;
using System.Globalization;
using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Logic.TwoFactor;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Authentication
{
    /// <summary>
    /// Rules of the login of a registered player.
    /// </summary>
    public class LoginServiceTests
    {
        private const int PlayerId = 1;
        private const string Email = "gabriel@gmail.com";
        private const string UnknownEmail = "carlos.ramirez@gmail.com";
        private const string MalformedEmail = "gabriel@";
        private const string Username = "gabriel.h";
        private const string UnknownUsername = "carlos.ramirez";
        private const string Password = "Secret123!";
        private const string WrongPassword = "Wrong123!";
        private const string DisplayName = "gabrielhernandez";
        private const string FirstName = "Gabriel";
        private const char FillCharacter = 'a';
        private const string GmailDomain = "@gmail.com";
        private const string SurroundingSpaces = "  ";

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryEmailService _emailService = new InMemoryEmailService();
        private readonly LoginService _service;

        /// <summary>
        /// Prepares the service with one registered player and without sending real emails.
        /// </summary>
        public LoginServiceTests()
        {
            var codeService = new TwoFactorCodeService(new InMemoryBackupCodeRepository(), _emailService);
            var verificationService = new TwoFactorVerificationService(_players, codeService);

            _players.Add(new Player
            {
                Id = PlayerId,
                Email = Email,
                Username = Username,
                PwdHash = CredentialRules.ComputeHash(Password),
                DisplayName = DisplayName,
                FirstName = FirstName
            });
            _service = new LoginService(_players, verificationService, codeService);
        }

        [Fact]
        public void LoginPlayer_ValidCredentials_ReturnsLoggedInWithTheNamesOfThePlayer()
        {
            LoginCredentials credentials = CreateCredentials(Email, Password);
            LoginResult loginResultExpected = new LoginResult(LoginStatus.LoggedIn, CreateKnownPlayer(), string.Empty);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equivalent(loginResultExpected, loginResult);
        }

        [Fact]
        public void LoginPlayer_WrongPassword_ReturnsInvalidCredentialsWithoutTheNamesOfThePlayer()
        {
            LoginCredentials credentials = CreateCredentials(Email, WrongPassword);
            LoginResult loginResultExpected = new LoginResult(LoginStatus.InvalidCredentials, string.Empty);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equivalent(loginResultExpected, loginResult);
        }

        [Fact]
        public void LoginPlayer_AccountSuspendedByReports_ReturnsSuspendedByReports()
        {
            _players.SaveLock(PlayerId, LoginLock.Reported.ToString(), DateTime.UtcNow.AddDays(1));

            LoginResult loginResult = _service.LoginPlayer(CreateCredentials(Email, Password));

            Assert.Equal(LoginStatus.SuspendedByReports, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_ThirdWrongPasswordInARow_ReturnsTemporarilyLocked()
        {
            LoginCredentials credentials = CreateCredentials(Email, WrongPassword);
            _service.LoginPlayer(credentials);
            _service.LoginPlayer(credentials);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.TemporarilyLocked, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_RightPasswordWhileLocked_ReturnsTemporarilyLocked()
        {
            LoginCredentials wrongCredentials = CreateCredentials(Email, WrongPassword);
            _service.LoginPlayer(wrongCredentials);
            _service.LoginPlayer(wrongCredentials);
            _service.LoginPlayer(wrongCredentials);

            LoginResult loginResult = _service.LoginPlayer(CreateCredentials(Email, Password));

            Assert.Equal(LoginStatus.TemporarilyLocked, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_UnknownEmail_ReturnsInvalidCredentials()
        {
            LoginCredentials credentials = CreateCredentials(UnknownEmail, Password);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.InvalidCredentials, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_UnknownEmailAndWrongPasswordOfARegisteredEmail_ReturnTheSameResult()
        {
            LoginResult unknownEmailResult = _service.LoginPlayer(CreateCredentials(UnknownEmail, WrongPassword));

            LoginResult wrongPasswordResult = _service.LoginPlayer(CreateCredentials(Email, WrongPassword));

            Assert.Equivalent(unknownEmailResult, wrongPasswordResult);
        }

        [Fact]
        public void LoginPlayer_PasswordLongerThanTheMaximum_ReturnsInvalidCredentials()
        {
            string longPassword = new string(FillCharacter, CredentialRules.PasswordMaxLength + 1);

            LoginResult loginResult = _service.LoginPlayer(CreateCredentials(Email, longPassword));

            Assert.Equal(LoginStatus.InvalidCredentials, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_EmailLongerThanTheMaximum_ReturnsInvalidEmail()
        {
            string longEmail = new string(FillCharacter, CredentialRules.EmailMaxLength) + GmailDomain;

            LoginResult loginResult = _service.LoginPlayer(CreateCredentials(longEmail, Password));

            Assert.Equal(LoginStatus.InvalidEmail, loginResult.Status);
        }

        [Theory]
        [InlineData("gabriel@")]
        [InlineData("@gmail.com")]
        [InlineData("gabriel@gmail .com")]
        public void LoginPlayer_MalformedEmail_ReturnsInvalidEmail(string email)
        {
            LoginCredentials credentials = CreateCredentials(email, Password);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.InvalidEmail, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_MalformedEmailAndWrongPassword_ReturnsInvalidEmail()
        {
            LoginCredentials credentials = CreateCredentials(MalformedEmail, WrongPassword);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.InvalidEmail, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_EmailWithSpacesAround_StillFindsThePlayer()
        {
            LoginCredentials credentials = CreateCredentials(SurroundingSpaces + Email + SurroundingSpaces, Password);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.LoggedIn, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_BlankPassword_ReturnsMissingRequiredField()
        {
            LoginCredentials credentials = CreateCredentials(Email, string.Empty);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.MissingRequiredField, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_TwoFactorEnabled_ReturnsVerificationCodeSent()
        {
            _players.SaveTwoFactorEnabled(PlayerId, true);
            LoginCredentials credentials = CreateCredentials(Email, Password);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.VerificationCodeSent, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_TwoFactorEnabled_SendsTheCodeToTheEmailOfThePlayer()
        {
            _players.SaveTwoFactorEnabled(PlayerId, true);
            LoginCredentials credentials = CreateCredentials(Email, Password);

            _service.LoginPlayer(credentials);

            Assert.Equal(Email, _emailService.Recipients.Single());
        }

        [Fact]
        public void LoginPlayer_ValidUsername_ReturnsLoggedInWithTheNamesOfThePlayer()
        {
            LoginCredentials credentials = CreateCredentials(Username, Password);
            LoginResult loginResultExpected = new LoginResult(LoginStatus.LoggedIn, CreateKnownPlayer(), string.Empty);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equivalent(loginResultExpected, loginResult);
        }

        [Fact]
        public void LoginPlayer_UsernameWithSpacesAround_StillFindsThePlayer()
        {
            LoginCredentials credentials = CreateCredentials(SurroundingSpaces + Username + SurroundingSpaces, Password);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.LoggedIn, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_UsernameWithWrongPassword_ReturnsInvalidCredentials()
        {
            LoginCredentials credentials = CreateCredentials(Username, WrongPassword);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.InvalidCredentials, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_UnknownUsername_ReturnsInvalidCredentials()
        {
            LoginCredentials credentials = CreateCredentials(UnknownUsername, Password);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.InvalidCredentials, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_UnknownUsernameAndWrongPasswordOfARegisteredUsername_ReturnTheSameResult()
        {
            LoginResult unknownUsernameResult =
                _service.LoginPlayer(CreateCredentials(UnknownUsername, WrongPassword));

            LoginResult wrongPasswordResult = _service.LoginPlayer(CreateCredentials(Username, WrongPassword));

            Assert.Equivalent(unknownUsernameResult, wrongPasswordResult);
        }

        [Theory]
        [InlineData("gabriel hernandez")]
        [InlineData("gabriel!")]
        [InlineData("gabriel\u00e9")]
        public void LoginPlayer_MalformedUsername_ReturnsInvalidCredentials(string username)
        {
            LoginCredentials credentials = CreateCredentials(username, Password);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.InvalidCredentials, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_UsernameLongerThanTheMaximum_ReturnsInvalidCredentials()
        {
            string longUsername = new string(FillCharacter, CredentialRules.UsernameMaxLength + 1);

            LoginResult loginResult = _service.LoginPlayer(CreateCredentials(longUsername, Password));

            Assert.Equal(LoginStatus.InvalidCredentials, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_TwoFactorEnabledAndUsername_ReturnsTheEmailOfTheAccount()
        {
            _players.SaveTwoFactorEnabled(PlayerId, true);
            LoginCredentials credentials = CreateCredentials(Username, Password);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(Email, loginResult.Email);
        }

        [Fact]
        public void LoginPlayer_TwoFactorEnabledAndUsername_SendsTheCodeToTheEmailOfThePlayer()
        {
            _players.SaveTwoFactorEnabled(PlayerId, true);
            LoginCredentials credentials = CreateCredentials(Username, Password);

            _service.LoginPlayer(credentials);

            Assert.Equal(Email, _emailService.Recipients.Single());
        }

        [Fact]
        public void LoginPlayer_ThirdWrongPasswordInARowWithUsername_ReturnsTemporarilyLocked()
        {
            LoginCredentials credentials = CreateCredentials(Username, WrongPassword);
            _service.LoginPlayer(credentials);
            _service.LoginPlayer(credentials);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.TemporarilyLocked, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_WrongPasswordsWithUsername_LockTheLoginWithTheEmailToo()
        {
            LoginCredentials wrongCredentials = CreateCredentials(Username, WrongPassword);
            _service.LoginPlayer(wrongCredentials);
            _service.LoginPlayer(wrongCredentials);
            _service.LoginPlayer(wrongCredentials);

            LoginResult loginResult = _service.LoginPlayer(CreateCredentials(Email, Password));

            Assert.Equal(LoginStatus.TemporarilyLocked, loginResult.Status);
        }

        [Fact]
        public void LoginPlayer_UsernameWithOtherCase_StillFindsThePlayer()
        {
            LoginCredentials credentials = CreateCredentials(Username.ToUpperInvariant(), Password);

            LoginResult loginResult = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.LoggedIn, loginResult.Status);
        }

        private static Player CreateKnownPlayer()
        {
            var player = new Player
            {
                Email = Email,
                DisplayName = DisplayName,
                FirstName = FirstName
            };

            return player;
        }

        private static LoginCredentials CreateCredentials(string identifier, string password)
        {
            var credentials = new LoginCredentials
            {
                Identifier = identifier,
                Password = password,
                Culture = CultureInfo.InvariantCulture
            };

            return credentials;
        }
    }
}
