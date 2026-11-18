using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.TwoFactor
{
    /// <summary>
    /// A passkey of the player, as the player sees it in the settings.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class PasskeyInfoData
    {
        /// <summary>
        /// Identifier that the client sends to remove the passkey.
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Name that helps the player recognize the passkey.
        /// </summary>
        [DataMember]
        public string Name { get; set; }
    }
}
