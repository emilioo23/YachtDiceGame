using System;
using System.Text;
using Xunit;
using YachtDice.Client.Logic.Passkeys;

namespace YachtDice.Client.Tests.Passkeys
{
    /// <summary>
    /// Rules of the client data that the game writes for the authenticator.
    /// </summary>
    public class PasskeyClientDataTests
    {
        private const string Challenge = "abc-_123";
        private const string ClientDataFormat =
            "{{\"type\":\"{0}\",\"challenge\":\"{1}\",\"origin\":\"{2}\",\"crossOrigin\":false}}";

        [Fact]
        public void Create_TypeAndChallenge_WritesTheJsonTheServerExpects()
        {
            string clientDataExpected = string.Format(
                ClientDataFormat,
                PasskeyRules.GetClientDataType,
                Challenge,
                PasskeyRules.Origin);

            byte[] clientData = PasskeyClientData.Create(PasskeyRules.GetClientDataType, Challenge);
            string clientDataResult = Encoding.UTF8.GetString(clientData);

            Assert.Equal(clientDataExpected, clientDataResult);
        }

        [Fact]
        public void Create_EmptyType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => PasskeyClientData.Create(string.Empty, Challenge));
        }

        [Fact]
        public void Create_EmptyChallenge_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => PasskeyClientData.Create(PasskeyRules.GetClientDataType, string.Empty));
        }
    }
}
