namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Kinds of authenticator that can hold a passkey, so the player can tell the passkeys apart in the list.
    /// </summary>
    public static class PasskeyKinds
    {
        /// <summary>
        /// Transport code that means that Windows did not report the authenticator.
        /// </summary>
        public const int UnknownTransport = 0;

        /// <summary>
        /// Transport code that Windows reports when the authenticator is the computer itself.
        /// </summary>
        public const int InternalTransport = 0x10;

        /// <summary>
        /// Transport code that Windows reports when the authenticator is a phone paired with the computer.
        /// </summary>
        public const int HybridTransport = 0x20;

        /// <summary>
        /// Transport code that Windows reports for a security key connected by USB.
        /// </summary>
        public const int UsbTransport = 0x01;

        /// <summary>
        /// Transport code that Windows reports for a security key read by NFC.
        /// </summary>
        public const int NfcTransport = 0x02;

        /// <summary>
        /// Transport code that Windows reports for a security key connected by Bluetooth.
        /// </summary>
        public const int BluetoothTransport = 0x04;

        /// <summary>
        /// Name of a passkey that lives in Windows Hello.
        /// </summary>
        public const string WindowsHelloName = "Windows Hello";

        /// <summary>
        /// Name of a passkey that lives in a phone.
        /// </summary>
        public const string PhoneName = "Celular";

        /// <summary>
        /// Name of a passkey that lives in a security key.
        /// </summary>
        public const string SecurityKeyName = "Llave de seguridad";

        /// <summary>
        /// Name of a passkey whose authenticator is not known.
        /// </summary>
        public const string UnknownName = "Passkey";

        /// <summary>
        /// Gets the name of the kind of authenticator that Windows reported.
        /// </summary>
        /// <param name="transport">Transport code that Windows reported; zero if it did not report one.</param>
        /// <returns>The name of the kind; the unknown name if the code is not recognized.</returns>
        public static string GetName(int transport)
        {
            string name = UnknownName;

            if (transport == InternalTransport)
            {
                name = WindowsHelloName;
            }
            else if (transport == HybridTransport)
            {
                name = PhoneName;
            }
            else if (transport == UsbTransport || transport == NfcTransport || transport == BluetoothTransport)
            {
                name = SecurityKeyName;
            }

            return name;
        }
    }
}
