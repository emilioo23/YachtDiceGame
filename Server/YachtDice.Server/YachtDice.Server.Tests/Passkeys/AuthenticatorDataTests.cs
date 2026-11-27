using System.Linq;
using System.Text;
using Xunit;
using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Rules of the reading of the authenticator data.
    /// </summary>
    public class AuthenticatorDataTests
    {
        private const string AuthenticatorDataKey = "authData";
        private const string ChallengeText = "AA";
        private const int ShortSize = 36;
        private const int FlagsOffset = 32;
        private const int CounterLastOffset = 36;
        private const byte CounterValue = 7;
        private const byte AttestedFlag = 0x40;
        private const int CredentialLengthOffset = 53;
        private const int CredentialLengthLowOffset = 54;
        private const int CredentialOffset = 55;
        private const int BitsPerByte = 8;
        private const int ByteMask = 0xFF;
        private const int ExtraByte = 1;

        [Fact]
        public void TryParse_DataShorterThanTheFixedPart_ReturnsFalse()
        {
            bool isParsed = AuthenticatorData.TryParse(new byte[ShortSize], out AuthenticatorData data);

            Assert.False(isParsed);
        }

        [Fact]
        public void TryParse_UserVerifiedFlag_MarksTheUserAsVerified()
        {
            AuthenticatorData.TryParse(CreateData(AuthenticatorOptions.PresentAndVerified), out AuthenticatorData data);

            Assert.True(data.IsUserVerified);
        }

        [Fact]
        public void TryParse_OnlyUserPresentFlag_DoesNotMarkTheUserAsVerified()
        {
            AuthenticatorData.TryParse(CreateData(AuthenticatorOptions.PresentOnly), out AuthenticatorData data);

            Assert.False(data.IsUserVerified);
        }

        [Fact]
        public void TryParse_CounterInTheLastByte_ReadsTheSignCount()
        {
            long signCountExpected = CounterValue;
            byte[] bytes = CreateData(AuthenticatorOptions.PresentAndVerified);

            bytes[CounterLastOffset] = CounterValue;
            AuthenticatorData.TryParse(bytes, out AuthenticatorData data);
            long signCountResult = data.SignCount;

            Assert.Equal(signCountExpected, signCountResult);
        }

        [Fact]
        public void TryParse_AttestedFlagWithoutCredential_ReturnsFalse()
        {
            byte[] bytes = CreateData((byte)(AuthenticatorOptions.PresentAndVerified | AttestedFlag));

            bool isParsed = AuthenticatorData.TryParse(bytes, out AuthenticatorData data);

            Assert.False(isParsed);
        }

        [Fact]
        public void TryParse_CredentialIdLargerThanTheMaximum_ReturnsFalse()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                int credentialLength = PasskeyRules.MaxCredentialIdSize + ExtraByte;
                byte[] bytes = ResizeCredentialId(ReadRegistrationData(authenticator), credentialLength);

                bool isParsed = AuthenticatorData.TryParse(bytes, out AuthenticatorData data);

                Assert.False(isParsed);
            }
        }

        [Fact]
        public void TryParse_CredentialIdOfTheMaximumSize_ReturnsTrue()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                int credentialLength = PasskeyRules.MaxCredentialIdSize;
                byte[] bytes = ResizeCredentialId(ReadRegistrationData(authenticator), credentialLength);

                bool isParsed = AuthenticatorData.TryParse(bytes, out AuthenticatorData data);

                Assert.True(isParsed);
            }
        }

        [Fact]
        public void TryParse_RegistrationData_ReadsTheCredentialId()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                byte[] credentialIdExpected = authenticator.CredentialId;
                byte[] bytes = ReadRegistrationData(authenticator);

                AuthenticatorData.TryParse(bytes, out AuthenticatorData data);
                byte[] credentialIdResult = data.CredentialId;

                Assert.Equal(credentialIdExpected, credentialIdResult);
            }
        }

        private static byte[] CreateData(byte flags)
        {
            byte[] data = new byte[CounterLastOffset + 1];

            data[FlagsOffset] = flags;

            return data;
        }

        private static byte[] ReadRegistrationData(FakeAuthenticator authenticator)
        {
            var challenge = new PasskeyChallenge(PasskeyStartStatus.Created) { Challenge = ChallengeText };
            PasskeyAttestation attestation = authenticator.CreateAttestation(challenge, new AuthenticatorOptions());
            PasskeyRules.TryDecode(attestation.AttestationObject, out byte[] attestationObject);
            int start = IndexOfAuthenticatorData(attestationObject);

            return attestationObject.Skip(start).ToArray();
        }

        private static byte[] ResizeCredentialId(byte[] registrationData, int credentialLength)
        {
            int currentLength = (registrationData[CredentialLengthOffset] << BitsPerByte)
                | registrationData[CredentialLengthLowOffset];
            byte[] head = registrationData.Take(CredentialOffset).ToArray();
            byte[] tail = registrationData.Skip(CredentialOffset + currentLength).ToArray();
            byte[] resized = head.Concat(new byte[credentialLength]).Concat(tail).ToArray();

            resized[CredentialLengthOffset] = (byte)(credentialLength >> BitsPerByte);
            resized[CredentialLengthLowOffset] = (byte)(credentialLength & ByteMask);

            return resized;
        }

        private static int IndexOfAuthenticatorData(byte[] attestationObject)
        {
            byte[] marker = Encoding.UTF8.GetBytes(AuthenticatorDataKey);
            int markerIndex = Enumerable.Range(0, attestationObject.Length - marker.Length)
                .First(index => attestationObject.Skip(index).Take(marker.Length).SequenceEqual(marker));
            const int HeaderAfterMarker = 2;

            return markerIndex + marker.Length + HeaderAfterMarker;
        }
    }
}
