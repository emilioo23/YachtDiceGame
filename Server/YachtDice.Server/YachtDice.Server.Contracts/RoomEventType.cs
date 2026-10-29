using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Types of events that can occur regarding players in a room.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum RoomEventType
    {
        [EnumMember]
        PlayerJoined,

        [EnumMember]
        PlayerLeft
    }
}