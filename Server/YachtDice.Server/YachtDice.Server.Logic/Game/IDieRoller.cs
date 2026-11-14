namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Gives the value of each die that is rolled. A game does not decide how the values are produced, so the
    /// tests can use fixed values.
    /// </summary>
    public interface IDieRoller
    {
        /// <summary>
        /// Rolls one die.
        /// </summary>
        /// <returns>A value from one to six.</returns>
        int Roll();
    }
}
