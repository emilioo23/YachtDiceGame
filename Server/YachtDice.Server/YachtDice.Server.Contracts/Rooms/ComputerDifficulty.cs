using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Rooms
{
    /// <summary>
    /// How well a computer player plays.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum ComputerDifficulty
    {
        /// <summary>Rolls once and writes the best box.</summary>
        [EnumMember]
        Easy,

        /// <summary>Keeps the dice that repeat the most.</summary>
        [EnumMember]
        Normal,

        /// <summary>Also keeps the dice of a straight that is almost complete.</summary>
        [EnumMember]
        Hard
    }
}
