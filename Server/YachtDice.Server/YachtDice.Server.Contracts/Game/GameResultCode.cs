using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Game
{
    /// <summary>
    /// Outcome of an action of a player in a game.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum GameResultCode
    {
        /// <summary>The action was done.</summary>
        [EnumMember]
        Success,

        /// <summary>There is no game in progress for the room.</summary>
        [EnumMember]
        GameNotFound,

        /// <summary>The player does not play in the game.</summary>
        [EnumMember]
        PlayerNotInGame,

        /// <summary>It is the turn of another player.</summary>
        [EnumMember]
        NotYourTurn,

        /// <summary>The game is already over.</summary>
        [EnumMember]
        GameFinished,

        /// <summary>The player already used the three rolls of the turn.</summary>
        [EnumMember]
        NoRollsLeft,

        /// <summary>The player has to roll the dice before choosing a box.</summary>
        [EnumMember]
        MustRollFirst,

        /// <summary>The list of held dice does not have one value for each die.</summary>
        [EnumMember]
        InvalidHeldDice,

        /// <summary>The box already has points.</summary>
        [EnumMember]
        CategoryFilled,

        /// <summary>The Joker rule does not allow to use the box with the current dice.</summary>
        [EnumMember]
        CategoryNotAllowed
    }
}
