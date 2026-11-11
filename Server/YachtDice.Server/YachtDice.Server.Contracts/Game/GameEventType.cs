using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Game
{
    /// <summary>
    /// Types of events that can happen in a game.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum GameEventType
    {
        /// <summary>The current player rolled the dice.</summary>
        [EnumMember]
        DiceRolled,

        /// <summary>The current player wrote the points in a box and the turn passed to another player.</summary>
        [EnumMember]
        CategoryScored,

        /// <summary>The time of the turn ran out, so the server played the turn.</summary>
        [EnumMember]
        TurnTimedOut,

        /// <summary>A player left the game or lost the connection, and can still come back.</summary>
        [EnumMember]
        PlayerDisconnected,

        /// <summary>A player who had left the game came back.</summary>
        [EnumMember]
        PlayerReturned,

        /// <summary>A player was removed from the game for good.</summary>
        [EnumMember]
        PlayerRemoved,

        /// <summary>The last turn was played and the game is over.</summary>
        [EnumMember]
        GameFinished
    }
}
