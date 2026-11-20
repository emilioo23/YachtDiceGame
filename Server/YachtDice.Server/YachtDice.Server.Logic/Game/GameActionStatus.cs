namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Outcome of an action of a player in a game.
    /// </summary>
    public enum GameActionStatus
    {
        /// <summary>The action was done.</summary>
        Success,

        /// <summary>There is no game in progress for the room.</summary>
        GameNotFound,

        /// <summary>The player does not play in the game.</summary>
        PlayerNotInGame,

        /// <summary>It is the turn of another player.</summary>
        NotYourTurn,

        /// <summary>The game is already over.</summary>
        GameFinished,

        /// <summary>The player already used the three rolls of the turn.</summary>
        NoRollsLeft,

        /// <summary>The player has to roll the dice before choosing a box.</summary>
        MustRollFirst,

        /// <summary>The list of held dice does not have one value for each die.</summary>
        InvalidHeldDice,

        /// <summary>The box already has points.</summary>
        CategoryFilled,

        /// <summary>The Joker rule does not allow to use the box with the current dice.</summary>
        CategoryNotAllowed
    }
}
