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
        public void LoginPlayer_ValidCredentials_ReturnsLoggedInWithThePlayerNames()
        {
            LoginCredentials credentials = CreateCredentials(Email, Password);

            LoginResult result = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.LoggedIn, result.Status);
            Assert.Equal(DisplayName, result.DisplayName);
            Assert.Equal(FirstName, result.FirstName);
        }

        [Fact]
        public void LoginPlayer_WrongPassword_ReturnsInvalidCredentialsWithoutNames()
        {
            LoginCredentials credentials = CreateCredentials(Email, WrongPassword);

            LoginResult result = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.InvalidCredentials, result.Status);
            Assert.Equal(string.Empty, result.DisplayName);
            Assert.Equal(string.Empty, result.FirstName);
        }

        [Fact]
        public void LoginPlayer_UnknownEmail_ReturnsInvalidCredentials()
        {
            LoginCredentials credentials = CreateCredentials(UnknownEmail, Password);

            LoginResult result = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.InvalidCredentials, result.Status);
        }

        [Fact]
        public void LoginPlayer_BlankPassword_ReturnsMissingRequiredField()
        {
            LoginCredentials credentials = CreateCredentials(Email, string.Empty);

            LoginResult result = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.MissingRequiredField, result.Status);
        }

        [Fact]
        public void LoginPlayer_TwoFactorEnabled_SendsTheCodeInsteadOfCompletingTheLogin()
        {
            _players.SaveTwoFactorEnabled(PlayerId, true);
            LoginCredentials credentials = CreateCredentials(Email, Password);

            LoginResult result = _service.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.VerificationCodeSent, result.Status);
            Assert.Equal(Email, _emailService.Recipients.Single());
        }

        private static LoginCredentials CreateCredentials(string email, string password)
        {
            return new LoginCredentials { Email = email, Password = password, Culture = CultureInfo.InvariantCulture };
        }
    }
}
