using Xunit;
using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Names and limits of the passkeys.
    /// </summary>
    public class PasskeyRulesTests
    {
        private const int UnrecognizedTransport = 0x40;

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
    }
}
