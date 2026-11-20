using Xunit;
using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Names that tell the player which authenticator holds each passkey.
    /// </summary>
    public class PasskeyKindsTests
    {
        private const int UnrecognizedTransport = 0x40;

        [Fact]
        public void GetName_InternalTransport_ReturnsWindowsHello()
        {
            string nameExpected = PasskeyKinds.WindowsHelloName;

            string nameResult = PasskeyKinds.GetName(PasskeyKinds.InternalTransport);

            Assert.Equal(nameExpected, nameResult);
        }

        [Fact]
        public void GetName_HybridTransport_ReturnsPhone()
        {
            string nameExpected = PasskeyKinds.PhoneName;

            string nameResult = PasskeyKinds.GetName(PasskeyKinds.HybridTransport);

            Assert.Equal(nameExpected, nameResult);
        }

        [Fact]
        public void GetName_UsbTransport_ReturnsSecurityKey()
        {
            string nameExpected = PasskeyKinds.SecurityKeyName;

            string nameResult = PasskeyKinds.GetName(PasskeyKinds.UsbTransport);

            Assert.Equal(nameExpected, nameResult);
        }

        [Fact]
        public void GetName_NfcTransport_ReturnsSecurityKey()
        {
            string nameExpected = PasskeyKinds.SecurityKeyName;

            string nameResult = PasskeyKinds.GetName(PasskeyKinds.NfcTransport);

            Assert.Equal(nameExpected, nameResult);
        }

        [Fact]
        public void GetName_BluetoothTransport_ReturnsSecurityKey()
        {
            string nameExpected = PasskeyKinds.SecurityKeyName;

            string nameResult = PasskeyKinds.GetName(PasskeyKinds.BluetoothTransport);

            Assert.Equal(nameExpected, nameResult);
        }

        [Fact]
        public void GetName_UnknownTransport_ReturnsPasskey()
        {
            string nameExpected = PasskeyKinds.UnknownName;

            string nameResult = PasskeyKinds.GetName(PasskeyKinds.UnknownTransport);

            Assert.Equal(nameExpected, nameResult);
        }

        [Fact]
        public void GetName_UnrecognizedTransport_ReturnsPasskey()
        {
            string nameExpected = PasskeyKinds.UnknownName;

            string nameResult = PasskeyKinds.GetName(UnrecognizedTransport);

            Assert.Equal(nameExpected, nameResult);
        }
    }
}
