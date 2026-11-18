using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Values that a test changes in the answer of the fake authenticator to make it invalid. By default the
    /// answer is a correct one.
    /// </summary>
    internal sealed class AuthenticatorOptions
    {
        /// <summary>
        /// Flags of the authenticator data: user present and user verified.
        /// </summary>
        public const byte PresentAndVerified = 0x05;

        /// <summary>
        /// Flags of the authenticator data: user present but not verified.
        /// </summary>
        public const byte PresentOnly = 0x01;

        /// <summary>
        /// Flags that the authenticator writes in the answer.
        /// </summary>
        public byte Flags { get; set; } = PresentAndVerified;

        /// <summary>
        /// Signature counter that the authenticator writes in the answer.
        /// </summary>
        public long SignCount { get; set; }

        /// <summary>
        /// Origin written in the client data.
        /// </summary>
        public string Origin { get; set; } = PasskeyRules.Origin;

        /// <summary>
        /// Identifier of the relying party that the authenticator hashes.
        /// </summary>
        public string RelyingPartyId { get; set; } = PasskeyRules.RelyingPartyId;

        /// <summary>
        /// Challenge written in the client data; null to use the one the server issued.
        /// </summary>
        public string Challenge { get; set; }

        /// <summary>
        /// Type written in the client data; null to use the one of the ceremony.
        /// </summary>
        public string ClientDataType { get; set; }

        /// <summary>
        /// Identifier of the player that the authenticator returns in a login.
        /// </summary>
        public string UserHandle { get; set; }
    }
}
