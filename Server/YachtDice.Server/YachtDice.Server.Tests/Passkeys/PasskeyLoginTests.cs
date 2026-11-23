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
        private const string OtherOrigin = "https://other.example";
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
            var challengeExpected = new { Status = PasskeyStartStatus.Created };

            PasskeyChallenge challenge = _context.Service.BeginLogin();

            Assert.Equivalent(challengeExpected, challenge);
        }

        [Fact]
        public void CompleteLogin_ValidAnswer_ReturnsLoggedInWithTheNamesOfThePlayer()
        {
            LoginResult loginResultExpected = new LoginResult(
                LoginStatus.LoggedIn,
                PasskeyTestContext.DisplayName,
                PasskeyTestContext.FirstName);

            LoginResult loginResult = Login(new AuthenticatorOptions { SignCount = NewerCounter });

            Assert.Equivalent(loginResultExpected, loginResult);
        }

        [Fact]
        public void CompleteLogin_ValidAnswer_SavesTheNewCounter()
        {
            long signCountExpected = NewerCounter;

            Login(new AuthenticatorOptions { SignCount = NewerCounter });
            long signCountResult = _context.Passkeys.Records[0].SignCount;

            Assert.Equal(signCountExpected, signCountResult);
        }

        [Fact]
        public void CompleteLogin_UserHandleOfThePlayer_ReturnsLoggedIn()
        {
            LoginStatus loginStatusExpected = LoginStatus.LoggedIn;
            PasskeyChallenge registration = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);
            var options = new AuthenticatorOptions { SignCount = NewerCounter, UserHandle = registration.UserHandle };

            LoginStatus loginStatusResult = Login(options).Status;

            Assert.Equal(loginStatusExpected, loginStatusResult);
        }

        [Fact]
        public void CompleteLogin_UserHandleOfAnotherPlayer_ReturnsInvalidPasskey()
        {
            LoginResult loginResultExpected = new LoginResult(LoginStatus.InvalidPasskey, string.Empty);
            var options = new AuthenticatorOptions
            {
                SignCount = NewerCounter,
                UserHandle = PasskeyRules.Encode(OtherPlayerHandle)
            };

            LoginResult loginResult = Login(options);

            Assert.Equivalent(loginResultExpected, loginResult);
        }

        [Fact]
        public void CompleteLogin_ChallengeUsedTwice_ReturnsInvalidPasskey()
        {
            LoginResult loginResultExpected = new LoginResult(LoginStatus.InvalidPasskey, string.Empty);
            PasskeyChallenge challenge = _context.Service.BeginLogin();
            PasskeyAssertion assertion = _authenticator.CreateAssertion(
                challenge,
                new AuthenticatorOptions { SignCount = NewerCounter });

            _context.Service.CompleteLogin(assertion);
            LoginResult loginResult = _context.Service.CompleteLogin(assertion);

            Assert.Equivalent(loginResultExpected, loginResult);
        }

        [Fact]
        public void CompleteLogin_ChallengeOfARegistration_ReturnsInvalidPasskey()
        {
            LoginResult loginResultExpected = new LoginResult(LoginStatus.InvalidPasskey, string.Empty);
            PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);
            PasskeyAssertion assertion = _authenticator.CreateAssertion(
                challenge,
                new AuthenticatorOptions { SignCount = NewerCounter });

            LoginResult loginResult = _context.Service.CompleteLogin(assertion);

            Assert.Equivalent(loginResultExpected, loginResult);
        }

        [Fact]
        public void CompleteLogin_CredentialThatIsNotRegistered_ReturnsInvalidPasskey()
        {
            using (FakeAuthenticator stranger = new FakeAuthenticator())
            {
                LoginResult loginResultExpected = new LoginResult(LoginStatus.InvalidPasskey, string.Empty);
                PasskeyChallenge challenge = _context.Service.BeginLogin();

                LoginResult loginResult = _context.Service.CompleteLogin(
                    stranger.CreateAssertion(challenge, new AuthenticatorOptions()));

                Assert.Equivalent(loginResultExpected, loginResult);
            }
        }

        [Fact]
        public void CompleteLogin_SignatureOfAnotherKeyWithTheSameCredential_ReturnsInvalidPasskey()
        {
            using (FakeAuthenticator impostor = new FakeAuthenticator())
            {
                LoginResult loginResultExpected = new LoginResult(LoginStatus.InvalidPasskey, string.Empty);
                PasskeyChallenge challenge = _context.Service.BeginLogin();
                PasskeyAssertion assertion = impostor.CreateAssertion(
                    challenge,
                    new AuthenticatorOptions { SignCount = NewerCounter });

                assertion.CredentialId = PasskeyRules.Encode(_authenticator.CredentialId);
                LoginResult loginResult = _context.Service.CompleteLogin(assertion);

                Assert.Equivalent(loginResultExpected, loginResult);
            }
        }

        [Fact]
        public void CompleteLogin_UserNotVerified_ReturnsInvalidPasskey()
        {
            LoginResult loginResultExpected = new LoginResult(LoginStatus.InvalidPasskey, string.Empty);
            var options = new AuthenticatorOptions
            {
                SignCount = NewerCounter,
                Flags = AuthenticatorOptions.PresentOnly
            };

            LoginResult loginResult = Login(options);

            Assert.Equivalent(loginResultExpected, loginResult);
        }

        [Fact]
        public void CompleteLogin_CounterThatWentBack_ReturnsInvalidPasskey()
        {
            LoginResult loginResultExpected = new LoginResult(LoginStatus.InvalidPasskey, string.Empty);

            LoginResult loginResult = Login(new AuthenticatorOptions { SignCount = OlderCounter });

            Assert.Equivalent(loginResultExpected, loginResult);
        }

        [Fact]
        public void CompleteLogin_ClientDataWithAnotherOrigin_ReturnsInvalidPasskey()
        {
            LoginResult loginResultExpected = new LoginResult(LoginStatus.InvalidPasskey, string.Empty);
            var options = new AuthenticatorOptions { SignCount = NewerCounter, Origin = OtherOrigin };

            LoginResult loginResult = Login(options);

            Assert.Equivalent(loginResultExpected, loginResult);
        }

        [Fact]
        public void CompleteLogin_AccountSuspendedByReports_ReturnsSuspendedByReports()
        {
            LoginResult loginResultExpected = new LoginResult(LoginStatus.SuspendedByReports, string.Empty);
            _context.Players.SaveLock(
                PasskeyTestContext.PlayerId,
                LoginLock.Reported.ToString(),
                DateTime.UtcNow.AddDays(OneDay));

            LoginResult loginResult = Login(new AuthenticatorOptions { SignCount = NewerCounter });

            Assert.Equivalent(loginResultExpected, loginResult);
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
