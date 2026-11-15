using System.Globalization;
using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Logic.TwoFactor;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.TwoFactor
{
    /// <summary>
    /// A new account confirms its email with the code that the first login sends.
    /// </summary>
    public class EmailVerificationTests
    {
        private const int PlayerId = 7;
        private const string Email = "ana.lopez@gmail.com";
        private const string Password = "Secret123!";
        private const string DisplayName = "analopez";
        private const string ChallengeId = "challenge-1";
        private const string CultureName = "es-MX";

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryEmailService _emailService = new InMemoryEmailService();
        private readonly TwoFactorCodeService _codeService = new TwoFactorCodeService();
        private readonly TwoFactorVerificationService _verificationService;
        private readonly LoginService _loginService;

        /// <summary>
        /// Prepares an account that registered and did not confirm its email yet.
        /// </summary>
        public EmailVerificationTests()
        {
            var deliveryService = new TwoFactorDeliveryService(_codeService, _emailService);

            _verificationService = new TwoFactorVerificationService(_players, _codeService, deliveryService);
            _loginService = new LoginService(_players, _verificationService, deliveryService);
            _players.Add(new Player
            {
                Id = PlayerId,
                Email = Email,
                PwdHash = PasswordHasher.ComputeHash(Password),
                DisplayName = DisplayName,
                State = PlayerState.Unverified.ToString()
            });
        }

        [Fact]
        public void LoginPlayer_UnverifiedAccount_SendsTheVerificationCode()
        {
            var credentials = new LoginCredentials
            {
                Email = Email,
                Password = Password,
                Culture = new CultureInfo(CultureName)
            };

            LoginResult result = _loginService.LoginPlayer(credentials);

            Assert.Equal(LoginStatus.VerificationCodeSent, result.Status);
        }

        [Fact]
        public void VerifyPlayerCode_RightCode_ActivatesTheAccount()
        {
            string code = _codeService.GenerateCode(Email, ChallengeId);

            _verificationService.VerifyPlayerCode(Email, new VerificationCode { Code = code, ChallengeId = ChallengeId });

            Assert.Equal(PlayerState.Active.ToString(), _players.Players.Single().State);
        }
    }
}
