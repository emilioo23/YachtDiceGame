using System.Text;
using Xunit;
using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Rules of the validation of the client data JSON of a ceremony.
    /// </summary>
    public class ClientDataValidatorTests
    {
        private const string OtherOrigin = "https://other.example";
        private const string ClientDataFormat = "{{\"type\":\"{0}\",\"challenge\":\"{1}\",\"origin\":\"{2}\"}}";
        private const string NotJson = "this is not json";
        private static readonly byte[] Challenge = { 1, 2, 3, 4 };
        private static readonly byte[] OtherChallenge = { 4, 3, 2, 1 };

        [Fact]
        public void IsValid_TypeChallengeAndOriginAsExpected_ReturnsTrue()
        {
            byte[] clientData = CreateClientData(PasskeyRules.GetClientDataType, Challenge, PasskeyRules.Origin);

            bool isValid = ClientDataValidator.IsValid(clientData, PasskeyRules.GetClientDataType, Challenge);

            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_TypeOfAnotherCeremony_ReturnsFalse()
        {
            byte[] clientData = CreateClientData(PasskeyRules.CreateClientDataType, Challenge, PasskeyRules.Origin);

            bool isValid = ClientDataValidator.IsValid(clientData, PasskeyRules.GetClientDataType, Challenge);

            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_ChallengeThatWasNotIssued_ReturnsFalse()
        {
            byte[] clientData = CreateClientData(PasskeyRules.GetClientDataType, OtherChallenge, PasskeyRules.Origin);

            bool isValid = ClientDataValidator.IsValid(clientData, PasskeyRules.GetClientDataType, Challenge);

            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_OriginOfAnotherSite_ReturnsFalse()
        {
            byte[] clientData = CreateClientData(PasskeyRules.GetClientDataType, Challenge, OtherOrigin);

            bool isValid = ClientDataValidator.IsValid(clientData, PasskeyRules.GetClientDataType, Challenge);

            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_TextThatIsNotJson_ReturnsFalse()
        {
            byte[] clientData = Encoding.UTF8.GetBytes(NotJson);

            bool isValid = ClientDataValidator.IsValid(clientData, PasskeyRules.GetClientDataType, Challenge);

            Assert.False(isValid);
        }

        private static byte[] CreateClientData(string type, byte[] challenge, string origin)
        {
            string json = string.Format(ClientDataFormat, type, Base64Url.Encode(challenge), origin);

            return Encoding.UTF8.GetBytes(json);
        }
    }
}
