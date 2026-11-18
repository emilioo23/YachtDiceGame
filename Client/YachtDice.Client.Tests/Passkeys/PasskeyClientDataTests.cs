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

        [Fact]
        public void Create_TypeAndChallenge_WritesTheJsonTheServerExpects()
        {
            byte[] data = PasskeyClientData.Create(PasskeyRules.GetClientDataType, Challenge);

            Assert.Equal(
                "{\"type\":\"webauthn.get\",\"challenge\":\"abc-_123\"," +
                "\"origin\":\"https://yachtdice.localhost\",\"crossOrigin\":false}",
                Encoding.UTF8.GetString(data));
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
