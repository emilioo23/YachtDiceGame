using System;
using Xunit;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Rules of the login with a passkey.
    /// </summary>
    public class PasskeyLoginTests : IDisposable
    {
        private const long RegisteredCounter = 5;
        private const long NewerCounter = 6;
        private const long OlderCounter = 4;
        private const int OneDay = 1;
        private static readonly byte[] OtherPlayerHandle = { 0, 0, 0, 2 };

        private readonly PasskeyTestContext _context = new PasskeyTestContext();
        private readonly FakeAuthenticator _authenticator = new FakeAuthenticator();

        /// <summary>
        /// Prepares a player who already has a passkey.
        /// </summary>
        public PasskeyLoginTests()
        {
            _context.Register(_authenticator, new AuthenticatorOptions { SignCount = RegisteredCounter });
        }

        /// <summary>
        /// Frees the authenticator.
        /// </summary>
        public void Dispose()
        {
            _authenticator.Dispose();
        }

        [Fact]
        public void BeginLogin_Always_ReturnsACreatedChallenge()
        {
            PasskeyChallenge challenge = _context.Service.BeginLogin();

            Assert.Equal(PasskeyStartStatus.Created, challenge.Status);
        }

        [Fact]
        public void CompleteLogin_ValidAnswer_ReturnsLoggedIn()
        {
            LoginResult result = Login(new AuthenticatorOptions { SignCount = NewerCounter });

            Assert.Equal(LoginStatus.LoggedIn, result.Status);
        }

        [Fact]
        public void CompleteLogin_ValidAnswer_ReturnsTheNameOfThePlayer()
        {
            LoginResult result = Login(new AuthenticatorOptions { SignCount = NewerCounter });

            Assert.Equal(PasskeyTestContext.DisplayName, result.DisplayName);
        }

        [Fact]
        public void CompleteLogin_ValidAnswer_SavesTheNewCounter()
        {
            Login(new AuthenticatorOptions { SignCount = NewerCounter });

            Assert.Equal(NewerCounter, _context.Passkeys.Records[0].SignCount);
        }

        [Fact]
        public void CompleteLogin_UserHandleOfThePlayer_ReturnsLoggedIn()
        {
            PasskeyChallenge registration = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);
            var options = new AuthenticatorOptions { SignCount = NewerCounter, UserHandle = registration.UserHandle };

            LoginResult result = Login(options);

            Assert.Equal(LoginStatus.LoggedIn, result.Status);
        }

        [Fact]
        public void CompleteLogin_UserHandleOfAnotherPlayer_ReturnsInvalidPasskey()
        {
            var options = new AuthenticatorOptions
            {
                SignCount = NewerCounter,
                UserHandle = Base64Url.Encode(OtherPlayerHandle)
            };

            LoginResult result = Login(options);

            Assert.Equal(LoginStatus.InvalidPasskey, result.Status);
        }

        [Fact]
        public void CompleteLogin_ChallengeUsedTwice_ReturnsInvalidPasskey()
        {
            PasskeyChallenge challenge = _context.Service.BeginLogin();
            PasskeyAssertion assertion = _authenticator.CreateAssertion(
                challenge,
                new AuthenticatorOptions { SignCount = NewerCounter });

            _context.Service.CompleteLogin(assertion);
            LoginResult result = _context.Service.CompleteLogin(assertion);

            Assert.Equal(LoginStatus.InvalidPasskey, result.Status);
        }

        [Fact]
        public void CompleteLogin_ChallengeOfARegistration_ReturnsInvalidPasskey()
        {
            PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);
            PasskeyAssertion assertion = _authenticator.CreateAssertion(
                challenge,
                new AuthenticatorOptions { SignCount = NewerCounter });

            LoginResult result = _context.Service.CompleteLogin(assertion);

            Assert.Equal(LoginStatus.InvalidPasskey, result.Status);
        }

        [Fact]
        public void CompleteLogin_CredentialThatIsNotRegistered_ReturnsInvalidPasskey()
        {
            using (var stranger = new FakeAuthenticator())
            {
                PasskeyChallenge challenge = _context.Service.BeginLogin();

                LoginResult result = _context.Service.CompleteLogin(
                    stranger.CreateAssertion(challenge, new AuthenticatorOptions()));

                Assert.Equal(LoginStatus.InvalidPasskey, result.Status);
            }
        }

        [Fact]
        public void CompleteLogin_SignatureOfAnotherKeyWithTheSameCredential_ReturnsInvalidPasskey()
        {
            using (var impostor = new FakeAuthenticator())
            {
                PasskeyChallenge challenge = _context.Service.BeginLogin();
                PasskeyAssertion assertion = impostor.CreateAssertion(
                    challenge,
                    new AuthenticatorOptions { SignCount = NewerCounter });

                assertion.CredentialId = Base64Url.Encode(_authenticator.CredentialId);
                LoginResult result = _context.Service.CompleteLogin(assertion);

                Assert.Equal(LoginStatus.InvalidPasskey, result.Status);
            }
        }

        [Fact]
        public void CompleteLogin_UserNotVerified_ReturnsInvalidPasskey()
        {
            var options = new AuthenticatorOptions
            {
                SignCount = NewerCounter,
                Flags = AuthenticatorOptions.PresentOnly
            };

            LoginResult result = Login(options);

            Assert.Equal(LoginStatus.InvalidPasskey, result.Status);
        }

        [Fact]
        public void CompleteLogin_CounterThatWentBack_ReturnsInvalidPasskey()
        {
            LoginResult result = Login(new AuthenticatorOptions { SignCount = OlderCounter });

            Assert.Equal(LoginStatus.InvalidPasskey, result.Status);
        }

        [Fact]
        public void CompleteLogin_ClientDataWithAnotherOrigin_ReturnsInvalidPasskey()
        {
            var options = new AuthenticatorOptions { SignCount = NewerCounter, Origin = "https://other.example" };

            LoginResult result = Login(options);

            Assert.Equal(LoginStatus.InvalidPasskey, result.Status);
        }

        [Fact]
        public void CompleteLogin_AccountSuspendedByReports_ReturnsSuspendedByReports()
        {
            _context.Players.SaveLock(
                PasskeyTestContext.PlayerId,
                LoginLock.Reported.ToString(),
                DateTime.UtcNow.AddDays(OneDay));

            LoginResult result = Login(new AuthenticatorOptions { SignCount = NewerCounter });

            Assert.Equal(LoginStatus.SuspendedByReports, result.Status);
        }

        [Fact]
        public void CompleteLogin_NullAnswer_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _context.Service.CompleteLogin(null));
        }

        private LoginResult Login(AuthenticatorOptions options)
        {
            PasskeyChallenge challenge = _context.Service.BeginLogin();

            return _context.Service.CompleteLogin(_authenticator.CreateAssertion(challenge, options));
        }
    }
}
