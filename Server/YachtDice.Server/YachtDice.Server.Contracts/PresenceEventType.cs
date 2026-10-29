using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Types of events that can occur regarding the players who are online.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum PresenceEventType
    {
        /// <summary>The player entered the game.</summary>
        [EnumMember]
        PlayerConnected,

        /// <summary>The player left the game or lost the connection.</summary>
        [EnumMember]
        PlayerDisconnected
    }
}
