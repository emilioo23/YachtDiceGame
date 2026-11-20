using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Game
{
    /// <summary>
    /// Phase of a game.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum GameStatus
    {
        /// <summary>The players are still playing.</summary>
        [EnumMember]
        InProgress,

        /// <summary>No present player has open boxes, so the game is over.</summary>
        [EnumMember]
        Finished
    }
}
