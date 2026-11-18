using System;
using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Rules of the creation of a passkey.
    /// </summary>
    public class PasskeyRegistrationTests
    {
        private const string UnknownName = "nobody";
        private const string OtherRelyingParty = "other.example";
        private const string GarbageAttestation = "AAAA";

        private readonly PasskeyTestContext _context = new PasskeyTestContext();

        [Fact]
        public void BeginRegistration_PlayerWithoutPasskeys_ReturnsACreatedChallenge()
        {
            PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);

            Assert.Equal(PasskeyStartStatus.Created, challenge.Status);
        }

        [Fact]
        public void BeginRegistration_UnknownPlayer_ReturnsPlayerNotFound()
        {
            PasskeyChallenge challenge = _context.Service.BeginRegistration(UnknownName);

            Assert.Equal(PasskeyStartStatus.PlayerNotFound, challenge.Status);
        }

        [Fact]
        public void BeginRegistration_PlayerWithTheMaximumOfPasskeys_ReturnsTooManyPasskeys()
        {
            RegisterPasskeys(PasskeyRules.MaxPasskeysPerPlayer);

            PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);

            Assert.Equal(PasskeyStartStatus.TooManyPasskeys, challenge.Status);
        }

        [Fact]
        public void BeginRegistration_PlayerWithAPasskey_ExcludesItsCredential()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                _context.Register(authenticator, new AuthenticatorOptions());

                PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);

                Assert.Equal(Base64Url.Encode(authenticator.CredentialId), challenge.ExcludedCredentialIds.Single());
            }
        }

        [Fact]
        public void CompleteRegistration_ValidAnswer_ReturnsRegistered()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                PasskeyRegistrationStatus status = _context.Register(authenticator, new AuthenticatorOptions());

                Assert.Equal(PasskeyRegistrationStatus.Registered, status);
            }
        }

        [Fact]
        public void CompleteRegistration_ValidAnswer_SavesThePasskeyOfThePlayer()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                _context.Register(authenticator, new AuthenticatorOptions());

                Assert.Equal(PasskeyTestContext.PlayerId, _context.Passkeys.Records.Single().PlayerId);
            }
        }

        [Fact]
        public void CompleteRegistration_ChallengeUsedTwice_ReturnsChallengeExpired()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);
                PasskeyAttestation attestation = authenticator.CreateAttestation(challenge, new AuthenticatorOptions());

                _context.Service.CompleteRegistration(PasskeyTestContext.DisplayName, attestation);
                PasskeyRegistrationStatus status =
                    _context.Service.CompleteRegistration(PasskeyTestContext.DisplayName, attestation);

                Assert.Equal(PasskeyRegistrationStatus.ChallengeExpired, status);
            }
        }

        [Fact]
        public void CompleteRegistration_ChallengeOfAnotherPlayer_ReturnsChallengeExpired()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.OtherDisplayName);
                PasskeyAttestation attestation = authenticator.CreateAttestation(challenge, new AuthenticatorOptions());

                PasskeyRegistrationStatus status =
                    _context.Service.CompleteRegistration(PasskeyTestContext.DisplayName, attestation);

                Assert.Equal(PasskeyRegistrationStatus.ChallengeExpired, status);
            }
        }

        [Fact]
        public void CompleteRegistration_ClientDataWithAnotherChallenge_ReturnsInvalidResponse()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                var options = new AuthenticatorOptions { Challenge = Base64Url.Encode(new byte[] { 1, 2, 3 }) };

                PasskeyRegistrationStatus status = _context.Register(authenticator, options);

                Assert.Equal(PasskeyRegistrationStatus.InvalidResponse, status);
            }
        }

        [Fact]
        public void CompleteRegistration_UserNotVerified_ReturnsInvalidResponse()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                var options = new AuthenticatorOptions { Flags = AuthenticatorOptions.PresentOnly };

                PasskeyRegistrationStatus status = _context.Register(authenticator, options);

                Assert.Equal(PasskeyRegistrationStatus.InvalidResponse, status);
            }
        }

        [Fact]
        public void CompleteRegistration_OtherRelyingParty_ReturnsInvalidResponse()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                var options = new AuthenticatorOptions { RelyingPartyId = OtherRelyingParty };

                PasskeyRegistrationStatus status = _context.Register(authenticator, options);

                Assert.Equal(PasskeyRegistrationStatus.InvalidResponse, status);
            }
        }

        [Fact]
        public void CompleteRegistration_AttestationThatIsNotCbor_ReturnsInvalidResponse()
        {
            PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);
            var attestation = new PasskeyAttestation
            {
                ChallengeId = challenge.ChallengeId,
                ClientDataJson = GarbageAttestation,
                AttestationObject = GarbageAttestation
            };

            PasskeyRegistrationStatus status =
                _context.Service.CompleteRegistration(PasskeyTestContext.DisplayName, attestation);

            Assert.Equal(PasskeyRegistrationStatus.InvalidResponse, status);
        }

        [Fact]
        public void CompleteRegistration_CredentialAlreadySaved_ReturnsAlreadyRegistered()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                _context.Register(authenticator, new AuthenticatorOptions());

                PasskeyRegistrationStatus status = _context.Register(authenticator, new AuthenticatorOptions());

                Assert.Equal(PasskeyRegistrationStatus.AlreadyRegistered, status);
            }
        }

        [Fact]
        public void CompleteRegistration_NullAnswer_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => _context.Service.CompleteRegistration(PasskeyTestContext.DisplayName, null));
        }

        [Fact]
        public void CountPasskeys_PlayerWithOnePasskey_ReturnsOne()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                _context.Register(authenticator, new AuthenticatorOptions());

                int count = _context.Service.CountPasskeys(PasskeyTestContext.DisplayName);

                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void Constructor_NullChallengeStore_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new PasskeyService(_context.Players, _context.Passkeys, null));
        }

        private void RegisterPasskeys(int count)
        {
            for (int index = 0; index < count; index++)
            {
                using (var authenticator = new FakeAuthenticator())
                {
                    _context.Register(authenticator, new AuthenticatorOptions());
                }
            }
        }
    }
}
