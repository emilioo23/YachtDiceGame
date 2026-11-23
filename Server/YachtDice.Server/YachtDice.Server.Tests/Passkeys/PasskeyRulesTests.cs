using System;
using System.Text;
using Xunit;
using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Tests of the rules and helpers of the passkeys.
    /// </summary>
    public class PasskeyRulesTests
    {
        private const int UnrecognizedTransport = 0x40;
        private const string UrlText = "-_8";
        private const string InvalidText = "not*base64";
        private const int PartSize = 4;
        private const string OtherOrigin = "https://other.example";
        private const string ClientDataFormat = "{{\"type\":\"{0}\",\"challenge\":\"{1}\",\"origin\":\"{2}\"}}";
        private const string NotJson = "this is not json";
        private static readonly byte[] BytesWithSpecialCharacters = { 0xFB, 0xFF };
        private static readonly byte[] DerWithLeadingZero =
            { 0x30, 0x0A, 0x02, 0x03, 0x00, 0x80, 0x01, 0x02, 0x03, 0x01, 0x02, 0x03 };
        private static readonly byte[] ExpectedFixed = { 0x00, 0x00, 0x80, 0x01, 0x00, 0x01, 0x02, 0x03 };
        private static readonly byte[] NotASequence = { 0x31, 0x00 };
        private static readonly byte[] WithExtraBytes = { 0x30, 0x06, 0x02, 0x01, 0x01, 0x02, 0x01, 0x01, 0xFF };
        private static readonly byte[] Challenge = { 1, 2, 3, 4 };
        private static readonly byte[] OtherChallenge = { 4, 3, 2, 1 };

        [Fact]
        public void GetName_InternalTransport_ReturnsWindowsHello()
        {
            string nameExpected = PasskeyRules.WindowsHelloName;

            string nameResult = PasskeyRules.GetName(PasskeyRules.InternalTransport);

            Assert.Equal(nameExpected, nameResult);
        }

        [Fact]
        public void GetName_HybridTransport_ReturnsPhone()
        {
            string nameExpected = PasskeyRules.PhoneName;

            string nameResult = PasskeyRules.GetName(PasskeyRules.HybridTransport);

            Assert.Equal(nameExpected, nameResult);
        }

        [Fact]
        public void GetName_UsbTransport_ReturnsSecurityKey()
        {
            string nameExpected = PasskeyRules.SecurityKeyName;

            string nameResult = PasskeyRules.GetName(PasskeyRules.UsbTransport);

            Assert.Equal(nameExpected, nameResult);
        }

        [Fact]
        public void GetName_NfcTransport_ReturnsSecurityKey()
        {
            string nameExpected = PasskeyRules.SecurityKeyName;

            string nameResult = PasskeyRules.GetName(PasskeyRules.NfcTransport);

            Assert.Equal(nameExpected, nameResult);
        }

        [Fact]
        public void GetName_BluetoothTransport_ReturnsSecurityKey()
        {
            string nameExpected = PasskeyRules.SecurityKeyName;

            string nameResult = PasskeyRules.GetName(PasskeyRules.BluetoothTransport);

            Assert.Equal(nameExpected, nameResult);
        }

        [Fact]
        public void GetName_UnknownTransport_ReturnsPasskey()
        {
            string nameExpected = PasskeyRules.UnknownName;

            string nameResult = PasskeyRules.GetName(PasskeyRules.UnknownTransport);

            Assert.Equal(nameExpected, nameResult);
        }

        [Fact]
        public void GetName_UnrecognizedTransport_ReturnsPasskey()
        {
            string nameExpected = PasskeyRules.UnknownName;

            string nameResult = PasskeyRules.GetName(UnrecognizedTransport);

            Assert.Equal(nameExpected, nameResult);
        }

        [Fact]
        public void Encode_BytesWithPlusAndSlash_UsesUrlCharactersWithoutPadding()
        {
            string textExpected = UrlText;

            string textResult = PasskeyRules.Encode(BytesWithSpecialCharacters);

            Assert.Equal(textExpected, textResult);
        }

        [Fact]
        public void TryDecode_EncodedBytes_ReturnsTheSameBytes()
        {
            byte[] bytesExpected = BytesWithSpecialCharacters;

            PasskeyRules.TryDecode(UrlText, out byte[] bytesResult);

            Assert.Equal(bytesExpected, bytesResult);
        }

        [Fact]
        public void TryDecode_TextWithInvalidCharacters_ReturnsFalse()
        {
            bool isValid = PasskeyRules.TryDecode(InvalidText, out byte[] bytes);

            Assert.False(isValid);
        }

        [Fact]
        public void TryDecode_EmptyText_ReturnsFalse()
        {
            bool isValid = PasskeyRules.TryDecode(string.Empty, out byte[] bytes);

            Assert.False(isValid);
        }

        [Fact]
        public void Encode_NullBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => PasskeyRules.Encode(null));
        }

        [Fact]
        public void TryConvert_DerWithLeadingZero_ReturnsRAndSPaddedToTheSameSize()
        {
            byte[] fixedSignatureExpected = ExpectedFixed;

            PasskeyRules.TryConvert(DerWithLeadingZero, PartSize, out byte[] fixedSignatureResult);

            Assert.Equal(fixedSignatureExpected, fixedSignatureResult);
        }

        [Fact]
        public void TryConvert_NotASequence_ReturnsFalse()
        {
            bool isConverted = PasskeyRules.TryConvert(NotASequence, PartSize, out byte[] fixedSignature);

            Assert.False(isConverted);
        }

        [Fact]
        public void TryConvert_BytesAfterTheSequence_ReturnsFalse()
        {
            bool isConverted = PasskeyRules.TryConvert(WithExtraBytes, PartSize, out byte[] fixedSignature);

            Assert.False(isConverted);
        }

        [Fact]
        public void IsValid_TypeChallengeAndOriginAsExpected_ReturnsTrue()
        {
            byte[] clientData = CreateClientData(PasskeyRules.GetClientDataType, Challenge, PasskeyRules.Origin);

            bool isValid = PasskeyRules.IsValid(clientData, PasskeyRules.GetClientDataType, Challenge);

            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_TypeOfAnotherCeremony_ReturnsFalse()
        {
            byte[] clientData = CreateClientData(PasskeyRules.CreateClientDataType, Challenge, PasskeyRules.Origin);

            bool isValid = PasskeyRules.IsValid(clientData, PasskeyRules.GetClientDataType, Challenge);

            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_ChallengeThatWasNotIssued_ReturnsFalse()
        {
            byte[] clientData = CreateClientData(PasskeyRules.GetClientDataType, OtherChallenge, PasskeyRules.Origin);

            bool isValid = PasskeyRules.IsValid(clientData, PasskeyRules.GetClientDataType, Challenge);

            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_OriginOfAnotherSite_ReturnsFalse()
        {
            byte[] clientData = CreateClientData(PasskeyRules.GetClientDataType, Challenge, OtherOrigin);

            bool isValid = PasskeyRules.IsValid(clientData, PasskeyRules.GetClientDataType, Challenge);

            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_TextThatIsNotJson_ReturnsFalse()
        {
            byte[] clientData = Encoding.UTF8.GetBytes(NotJson);

            bool isValid = PasskeyRules.IsValid(clientData, PasskeyRules.GetClientDataType, Challenge);

            Assert.False(isValid);
        }

        private static byte[] CreateClientData(string type, byte[] challenge, string origin)
        {
            string json = string.Format(ClientDataFormat, type, PasskeyRules.Encode(challenge), origin);

            return Encoding.UTF8.GetBytes(json);
        }
    }
}
