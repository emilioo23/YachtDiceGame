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
        private const string Email = "gabriel@example.com";
        private const string UnknownEmail = "nobody@example.com";
        private const string MalformedEmail = "gabriel";
        private const string Password = "Secret123!";
        private const string WrongPassword = "Wrong123!";
        private const string DisplayName = "gabrielhernandez";
        private const string FirstName = "Gabriel";

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryEmailService _emailService = new InMemoryEmailService();
        private readonly LoginService _service;

        /// <summary>
        /// Prepares the service with one registered player and without sending real emails.
        /// </summary>
        public LoginServiceTests()
        {
            var codeService = new TwoFactorCodeService();
            var deliveryService = new TwoFactorDeliveryService(codeService, _emailService);
            var verificationService = new TwoFactorVerificationService(_players, codeService, deliveryService);

            _players.Add(new Player
            {
                Id = PlayerId,
                Email = Email,
                PwdHash = PasswordHasher.ComputeHash(Password),
                DisplayName = DisplayName,
                FirstName = FirstName
            });
            _service = new LoginService(_players, verificationService, deliveryService);
        }

        [Fact]
        public void LoginPlayer_ValidCredentials_ReturnsLoggedInWithTheNamesOfThePlayer()
        {
            LoginCredentials credentials = CreateCredentials(Email, Password);
            LoginResult expected = new LoginResult(LoginStatus.LoggedIn, CreateKnownPlayer(), string.Empty);

            LoginResult result = _service.LoginPlayer(credentials);

            Assert.Equivalent(expected, result);
        }

        [Fact]
        public void LoginPlayer_WrongPassword_ReturnsInvalidPasswordWithoutTheNamesOfThePlayer()
        {
            LoginCredentials credentials = CreateCredentials(Email, WrongPassword);
            LoginResult expected = new LoginResult(LoginStatus.InvalidPassword, string.Empty);

            LoginResult result = _service.LoginPlayer(credentials);

            Assert.Equivalent(expected, result);
        }

        [Fact]
        public void LoginPlayer_UnknownEmail_ReturnsEmailNotRegistered()
        {
            LoginCredentials credentials = CreateCredentials(UnknownEmail, Password);

            LoginResult result = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.EmailNotRegistered, result.Status);
        }

        [Theory]
        [InlineData("gabriel")]
        [InlineData("gabriel@")]
        [InlineData("gabriel example.com")]
        public void LoginPlayer_MalformedEmail_ReturnsInvalidEmail(string email)
        {
            LoginCredentials credentials = CreateCredentials(email, Password);

            LoginResult result = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.InvalidEmail, result.Status);
        }

        [Fact]
        public void LoginPlayer_MalformedEmailAndWrongPassword_ReturnsInvalidEmail()
        {
            LoginCredentials credentials = CreateCredentials(MalformedEmail, WrongPassword);

            LoginResult result = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.InvalidEmail, result.Status);
        }

        [Fact]
        public void LoginPlayer_EmailWithSpacesAround_StillFindsThePlayer()
        {
            LoginCredentials credentials = CreateCredentials("  " + Email + "  ", Password);

            LoginResult result = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.LoggedIn, result.Status);
        }

        [Fact]
        public void LoginPlayer_BlankPassword_ReturnsMissingRequiredField()
        {
            LoginCredentials credentials = CreateCredentials(Email, string.Empty);

            LoginResult result = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.MissingRequiredField, result.Status);
        }

        [Fact]
        public void LoginPlayer_TwoFactorEnabled_ReturnsVerificationCodeSent()
        {
            _players.SaveTwoFactorEnabled(PlayerId, true);
            LoginCredentials credentials = CreateCredentials(Email, Password);

            LoginResult result = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.VerificationCodeSent, result.Status);
        }

        [Fact]
        public void LoginPlayer_TwoFactorEnabled_SendsTheCodeToTheEmailOfThePlayer()
        {
            _players.SaveTwoFactorEnabled(PlayerId, true);
            LoginCredentials credentials = CreateCredentials(Email, Password);

            _service.LoginPlayer(credentials);

            Assert.Equal(Email, _emailService.Recipients.Single());
        }

        private static Player CreateKnownPlayer()
        {
            var player = new Player
            {
                DisplayName = DisplayName,
                FirstName = FirstName
            };

            return player;
        }

        private static LoginCredentials CreateCredentials(string email, string password)
        {
            var credentials = new LoginCredentials
            {
                Email = email,
                Password = password,
                Culture = CultureInfo.InvariantCulture
            };

            return credentials;
        }
    }
}
