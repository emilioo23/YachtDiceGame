using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Rooms
{
    /// <summary>
    /// How fast the turns of a game are.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum GameSpeed
    {
        /// <summary>Normal pace, one minute per turn.</summary>
        [EnumMember]
        Normal,

        /// <summary>Fast pace, thirty seconds per turn.</summary>
        [EnumMember]
        Fast
    }
}
