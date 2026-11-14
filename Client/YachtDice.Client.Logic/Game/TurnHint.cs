namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// Which hint helps the player in the current moment of the turn.
    /// </summary>
    public enum TurnHint
    {
        /// <summary>
        /// No hint is needed.
        /// </summary>
        None,

        /// <summary>
        /// The player has not rolled yet and must press the roll button.
        /// </summary>
        PressRoll,

        /// <summary>
        /// The player can choose a box or keep dice and roll again.
        /// </summary>
        ChooseOrRoll,

        /// <summary>
        /// The player has no rolls left and must pick a box.
        /// </summary>
        PickBox,

        /// <summary>
        /// The player waits for another player.
        /// </summary>
        WaitOpponent
    }
}
