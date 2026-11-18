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
        private const string GarbageText = "AAAA";
        private const int OnePasskey = 1;
        private static readonly byte[] OtherChallenge = { 1, 2, 3 };

        private readonly PasskeyTestContext _context = new PasskeyTestContext();

        [Fact]
        public void BeginRegistration_PlayerWithoutPasskeys_ReturnsACreatedChallenge()
        {
            var challengeExpected = new { Status = PasskeyStartStatus.Created };

            PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);

            Assert.Equivalent(challengeExpected, challenge);
        }

        [Fact]
        public void BeginRegistration_UnknownPlayer_ReturnsPlayerNotFound()
        {
            var challengeExpected = new { Status = PasskeyStartStatus.PlayerNotFound };

            PasskeyChallenge challenge = _context.Service.BeginRegistration(UnknownName);

            Assert.Equivalent(challengeExpected, challenge);
        }

        [Fact]
        public void BeginRegistration_PlayerWithTheMaximumOfPasskeys_ReturnsTooManyPasskeys()
        {
            var challengeExpected = new { Status = PasskeyStartStatus.TooManyPasskeys };
            RegisterPasskeys(PasskeyRules.MaxPasskeysPerPlayer);

            PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);

            Assert.Equivalent(challengeExpected, challenge);
        }

        [Fact]
        public void BeginRegistration_PlayerWithAPasskey_ExcludesItsCredential()
        {
            using (FakeAuthenticator authenticator = new FakeAuthenticator())
            {
                var challengeExpected = new
                {
                    ExcludedCredentialIds = new[] { Base64Url.Encode(authenticator.CredentialId) }
                };
                _context.Register(authenticator, new AuthenticatorOptions());

                PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);

                Assert.Equivalent(challengeExpected, challenge);
            }
        }

        [Fact]
        public void CompleteRegistration_ValidAnswer_ReturnsRegistered()
        {
            using (FakeAuthenticator authenticator = new FakeAuthenticator())
            {
                PasskeyRegistrationStatus registrationStatusExpected = PasskeyRegistrationStatus.Registered;

                PasskeyRegistrationStatus registrationStatusResult =
                    _context.Register(authenticator, new AuthenticatorOptions());

                Assert.Equal(registrationStatusExpected, registrationStatusResult);
            }
        }

        [Fact]
        public void CompleteRegistration_ValidAnswer_SavesThePasskeyOfThePlayer()
        {
            using (FakeAuthenticator authenticator = new FakeAuthenticator())
            {
                var passkeyExpected = new
                {
                    PlayerId = PasskeyTestContext.PlayerId,
                    CredentialId = Base64Url.Encode(authenticator.CredentialId)
                };

                _context.Register(authenticator, new AuthenticatorOptions());

                Assert.Equivalent(passkeyExpected, _context.Passkeys.Records.Single());
            }
        }

        [Fact]
        public void CompleteRegistration_ChallengeUsedTwice_ReturnsChallengeExpired()
        {
            using (FakeAuthenticator authenticator = new FakeAuthenticator())
            {
                PasskeyRegistrationStatus registrationStatusExpected = PasskeyRegistrationStatus.ChallengeExpired;
                PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);
                PasskeyAttestation attestation = authenticator.CreateAttestation(challenge, new AuthenticatorOptions());

                _context.Service.CompleteRegistration(PasskeyTestContext.DisplayName, attestation);
                PasskeyRegistrationStatus registrationStatusResult =
                    _context.Service.CompleteRegistration(PasskeyTestContext.DisplayName, attestation);

                Assert.Equal(registrationStatusExpected, registrationStatusResult);
            }
        }

        [Fact]
        public void CompleteRegistration_ChallengeOfAnotherPlayer_ReturnsChallengeExpired()
        {
            using (FakeAuthenticator authenticator = new FakeAuthenticator())
            {
                PasskeyRegistrationStatus registrationStatusExpected = PasskeyRegistrationStatus.ChallengeExpired;
                PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.OtherDisplayName);
                PasskeyAttestation attestation = authenticator.CreateAttestation(challenge, new AuthenticatorOptions());

                PasskeyRegistrationStatus registrationStatusResult =
                    _context.Service.CompleteRegistration(PasskeyTestContext.DisplayName, attestation);

                Assert.Equal(registrationStatusExpected, registrationStatusResult);
            }
        }

        [Fact]
        public void CompleteRegistration_ClientDataWithAnotherChallenge_ReturnsInvalidResponse()
        {
            using (FakeAuthenticator authenticator = new FakeAuthenticator())
            {
                PasskeyRegistrationStatus registrationStatusExpected = PasskeyRegistrationStatus.InvalidResponse;
                var options = new AuthenticatorOptions { Challenge = Base64Url.Encode(OtherChallenge) };

                PasskeyRegistrationStatus registrationStatusResult = _context.Register(authenticator, options);

                Assert.Equal(registrationStatusExpected, registrationStatusResult);
            }
        }

        [Fact]
        public void CompleteRegistration_UserNotVerified_ReturnsInvalidResponse()
        {
            using (FakeAuthenticator authenticator = new FakeAuthenticator())
            {
                PasskeyRegistrationStatus registrationStatusExpected = PasskeyRegistrationStatus.InvalidResponse;
                var options = new AuthenticatorOptions { Flags = AuthenticatorOptions.PresentOnly };

                PasskeyRegistrationStatus registrationStatusResult = _context.Register(authenticator, options);

                Assert.Equal(registrationStatusExpected, registrationStatusResult);
            }
        }

        [Fact]
        public void CompleteRegistration_OtherRelyingParty_ReturnsInvalidResponse()
        {
            using (FakeAuthenticator authenticator = new FakeAuthenticator())
            {
                PasskeyRegistrationStatus registrationStatusExpected = PasskeyRegistrationStatus.InvalidResponse;
                var options = new AuthenticatorOptions { RelyingPartyId = OtherRelyingParty };

                PasskeyRegistrationStatus registrationStatusResult = _context.Register(authenticator, options);

                Assert.Equal(registrationStatusExpected, registrationStatusResult);
            }
        }

        [Fact]
        public void CompleteRegistration_AttestationThatIsNotCbor_ReturnsInvalidResponse()
        {
            PasskeyRegistrationStatus registrationStatusExpected = PasskeyRegistrationStatus.InvalidResponse;
            PasskeyChallenge challenge = _context.Service.BeginRegistration(PasskeyTestContext.DisplayName);
            var attestation = new PasskeyAttestation
            {
                ChallengeId = challenge.ChallengeId,
                ClientDataJson = GarbageText,
                AttestationObject = GarbageText
            };

            PasskeyRegistrationStatus registrationStatusResult =
                _context.Service.CompleteRegistration(PasskeyTestContext.DisplayName, attestation);

            Assert.Equal(registrationStatusExpected, registrationStatusResult);
        }

        [Fact]
        public void CompleteRegistration_CredentialAlreadySaved_ReturnsAlreadyRegistered()
        {
            using (FakeAuthenticator authenticator = new FakeAuthenticator())
            {
                PasskeyRegistrationStatus registrationStatusExpected = PasskeyRegistrationStatus.AlreadyRegistered;
                _context.Register(authenticator, new AuthenticatorOptions());

                PasskeyRegistrationStatus registrationStatusResult =
                    _context.Register(authenticator, new AuthenticatorOptions());

                Assert.Equal(registrationStatusExpected, registrationStatusResult);
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
            using (FakeAuthenticator authenticator = new FakeAuthenticator())
            {
                int passkeyCountExpected = OnePasskey;
                _context.Register(authenticator, new AuthenticatorOptions());

                int passkeyCountResult = _context.Service.CountPasskeys(PasskeyTestContext.DisplayName);

                Assert.Equal(passkeyCountExpected, passkeyCountResult);
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
                using (FakeAuthenticator authenticator = new FakeAuthenticator())
                {
                    _context.Register(authenticator, new AuthenticatorOptions());
                }
            }
        }
    }
}
